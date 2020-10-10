using System;
using System.Collections.Generic;
using System.Linq;
using avaness.ToolSwitcher.Tools;
using DarkHelmet.BuildVision2;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.Utils;

namespace avaness.ToolSwitcher
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ToolSwitcherSession : MySessionComponentBase
    {
        public static ToolSwitcherSession Instance;
        
        private bool init = false;
        private bool equipAll = false;
        private readonly List<EquippedToolAction> equippedTools = new List<EquippedToolAction>();
        private ToolGroups config;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            BvApiClient.Init(DebugName);
        }

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            Instance = null;
            config?.Unload();
            MyVisualScriptLogicProvider.ToolbarItemChanged -= ToolbarItemChanged;
        }

        private void Start()
        {
            init = true;
            config = ToolGroups.Load();
            MyVisualScriptLogicProvider.ToolbarItemChanged += ToolbarItemChanged;
            if (!MyAPIGateway.Session.CreativeMode)
                equipAll = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session?.Player == null)
                return;

            if (!init)
                Start();


            foreach (EquippedToolAction toolAction in equippedTools.ToArray())
                toolAction.Do(config);
            equippedTools.Clear();

            if(MyAPIGateway.Session.Player.Character != null)
            {
                bool toolbar = IsToolbarCharacter();
                config.MenuEnabled = toolbar;
                if (toolbar && IsEnabled())
                {
                    if (MyAPIGateway.Input.IsNewKeyPressed(config.EquipAllKey))
                        equipAll = true;

                    if (equipAll)
                    {
                        foreach (ToolGroup g in config)
                            g.First().Equip();
                        equipAll = false;
                    }
                    else
                    {
                        foreach (ToolGroup g in config)
                            g.HandleInput();
                    }
                }
            }
            else
            {
                config.MenuEnabled = false;
            }

        }

        private void ToolbarItemChanged(long entityId, string typeId, string subtypeId, int page, int slot)
        {
            if (MyAPIGateway.Session.Player == null)
                return;

            MyDefinitionId handId;
            if(IsToolbarCharacter() && MyAPIGateway.Gui.ActiveGamePlayScreen == "MyGuiScreenCubeBuilder" 
                && typeId == "MyObjectBuilder_PhysicalGunObject" && MyDefinitionId.TryParse(typeId, subtypeId, out handId))
            {
                EquippedToolAction newToolAction = new EquippedToolAction(handId, page, slot);
                foreach (EquippedToolAction toolAction in equippedTools)
                {
                    if (toolAction.Update(newToolAction))
                        return;
                }
                equippedTools.Add(newToolAction);
            }
        }

        public static bool IsToolbarCharacter()
        {
            if (MyAPIGateway.Session.Player.Character == null)
                return false;
            return MyAPIGateway.Session.Player.Character.Parent == null && MyAPIGateway.Session.Player.Controller.ControlledEntity is IMyCharacter;
        }

        private static bool IsEnabled()
        {
            return MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None && !MyAPIGateway.Gui.IsCursorVisible && !MyAPIGateway.Gui.ChatEntryVisible 
                && !MyAPIGateway.Session.IsCameraUserControlledSpectator && string.IsNullOrWhiteSpace(MyAPIGateway.Gui.ActiveGamePlayScreen) && (!BvApiClient.Registered || !BvApiClient.Open);
        }

        public static string GetKeyName(MyKeys key)
        {
            if (key == MyKeys.None)
                return "None";
            return MyAPIGateway.Input.GetKeyName(key);
        }

        private class EquippedToolAction
        {
            private readonly MyDefinitionId id;
            private readonly int slot;
            private int page;

            public EquippedToolAction(MyDefinitionId id, int page, int slot)
            {
                this.id = id;
                this.page = page;
                this.slot = slot;
            }

            public bool Update(EquippedToolAction other)
            {
                if (id != other.id || slot != other.slot)
                    return false;
                // The reason for these shenanigans: Keen's code sometimes uses page=0 in the parameter.
                if (page == 0 && other.page != 0)
                    page = other.page;
                return true;
            }

            public void Do(ToolGroups config)
            {
                foreach (ToolGroup g in config)
                {
                    Tool t;
                    if (g.TryGetTool(id, out t))
                    {
                        if (!t.Enabled)
                            return;

                        if (g.IsSlot(page, slot))
                        {
                            t.Upgrade(id);
                        }
                        else
                        {
                            t.ClearSlot();
                            t.Page = page;
                            t.Slot = slot;
                            if (t.Menu != null)
                                t.Menu.SlotUpdated();
                            config.ToolEdited(t);
                            config.Save();
                        }
                        return;
                    }
                }
            }
        }
    }
}