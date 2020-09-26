using System;
using System.Collections.Generic;
using System.Linq;
using avaness.ToolSwitcher.Tools;
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

        private ToolGroups config;

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
            equipAll = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session?.Player == null)
                return;
            if (!init)
                Start();

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
                foreach (ToolGroup g in config)
                {
                    Tool t;
                    if (g.TryGetTool(handId, out t) && !g.IsSlot(page, slot))
                    {
                        t.ClearSlot();
                        t.Page = page;
                        t.Slot = slot;
                        config.ToolEdited(t);
                        config.Save();
                        return;
                    }
                }
            }
        }

        public static bool IsToolbarCharacter()
        {
            return MyAPIGateway.Session.Player.Character.Parent == null;
        }

        private static bool IsEnabled()
        {
            return MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None && !MyAPIGateway.Gui.IsCursorVisible && !MyAPIGateway.Gui.ChatEntryVisible 
                && !MyAPIGateway.Session.IsCameraUserControlledSpectator && string.IsNullOrWhiteSpace(MyAPIGateway.Gui.ActiveGamePlayScreen);
        }

        public static string GetKeyName(MyKeys key)
        {
            if (key == MyKeys.None)
                return "None";
            return MyAPIGateway.Input.GetKeyName(key);
        }
    }
}