using System;
using System.Collections.Generic;
using avaness.ToolSwitcher.Net;
using avaness.ToolSwitcher.Tools;
using DarkHelmet.BuildVision2;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
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
        private bool modOverrideEnabled = true;

        private bool IsServer => MyAPIGateway.Session.IsServer || MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE;
        private bool IsClient => !IsServer || !MyAPIGateway.Utilities.IsDedicated;

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
            MyVisualScriptLogicProvider.PlayerDropped -= ItemDropped;
            MyVisualScriptLogicProvider.PlayerPickedUp -= ItemPickedUp;
            MyVisualScriptLogicProvider.PlayerSpawned -= PlayerSpawned;
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(EventPacket.PacketId, EventPacket.Received);
            equippedTools.Clear();
        }

        public override void BeforeStart()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(2211605465, ReceiveModMessage);
        }

        private void ReceiveModMessage(object obj)
        {
            if (obj is bool)
                modOverrideEnabled = (bool)obj;
        }

        private void Start()
        {
            init = true;

            if(IsServer)
            {
                MyVisualScriptLogicProvider.PlayerDropped += ItemDropped;
                MyVisualScriptLogicProvider.PlayerPickedUp += ItemPickedUp;
                MyVisualScriptLogicProvider.PlayerSpawned += PlayerSpawned;
            }
            else
            {
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(EventPacket.PacketId, EventPacket.Received);
            }

            if(IsClient)
            {
                config = ToolGroups.Load();
                MyVisualScriptLogicProvider.ToolbarItemChanged += ToolbarItemChanged;
                if (!MyAPIGateway.Session.CreativeMode)
                    equipAll = true;
            }
        }

        private void PlayerSpawned(long playerId)
        {
            if(modOverrideEnabled)
                new EventPacket(EventPacket.Mode.Spawn, new MyDefinitionId()).SendTo(playerId);
        }

        private void ItemPickedUp(string itemTypeName, string itemSubTypeName, string entityName, long playerId, int amount)
        {
            MyDefinitionId handId;
            if (modOverrideEnabled && amount == 1 && itemTypeName == "MyObjectBuilder_PhysicalGunObject" && MyDefinitionId.TryParse(itemTypeName, itemSubTypeName, out handId))
                new EventPacket(EventPacket.Mode.PickUp, handId).SendTo(playerId);
        }

        private void ItemDropped(string itemTypeName, string itemSubTypeName, long playerId, int amount)
        {
            MyDefinitionId handId;
            if (modOverrideEnabled && amount == 1 && itemTypeName == "MyObjectBuilder_PhysicalGunObject" && MyDefinitionId.TryParse(itemTypeName, itemSubTypeName, out handId))
                new EventPacket(EventPacket.Mode.Drop, handId).SendTo(playerId);
        }

        public void EquipAll()
        {
            if(modOverrideEnabled)
                equipAll = true;
        }

        public void CheckItem(MyDefinitionId id, bool added)
        {
            Tool t;
            ToolGroup tg;
            int index;
            if (modOverrideEnabled && config.ModEnabled && IsToolbarCharacter() && config.TryGetTool(id, out t, out index, out tg) && t.Enabled)
            {
                if (added)
                    t.Equip();
                else if (!t.Equip(id))
                    tg.EquipAny(id);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;

            if (IsClient && MyAPIGateway.Session.Player == null)
                return;

            if (!init)
                Start();

            if (IsClient)
            {
                foreach (EquippedToolAction toolAction in equippedTools.ToArray())
                    toolAction.Do(config);
                equippedTools.Clear();

                if (MyAPIGateway.Session.Player.Character != null)
                {
                    bool toolbar = IsToolbarCharacter();
                    config.MenuEnabled = toolbar;
                    if (toolbar && IsEnabled())
                    {
                        IMyInput input = MyAPIGateway.Input;
                        
                        if (equipAll || input.IsNewKeyPressed(config.EquipAllKey))
                        {
                            foreach (ToolGroup g in config)
                                g.EquipAny();
                            equipAll = false;
                        }
                        else if(input.IsNewKeyPressed(config.UpgradeKey))
                        {
                            Tool t;
                            ToolGroup tg;
                            if (config.TryGetTool(out t, out tg))
                                t.Upgrade();
                        }
                        else if(input.IsNewKeyPressed(config.DowngradeKey))
                        {
                            Tool t;
                            ToolGroup tg;
                            if (config.TryGetTool(out t, out tg))
                                t.Downgrade();
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
        }

        private void ToolbarItemChanged(long entityId, string typeId, string subtypeId, int page, int slot)
        {
            try
            {
                if (MyAPIGateway.Session.Player == null || !modOverrideEnabled)
                    return;

                MyDefinitionId handId;
                if (config.ModEnabled && IsToolbarCharacter() && MyAPIGateway.Gui.ActiveGamePlayScreen == "MyGuiScreenCubeBuilder"
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
            catch (Exception e) 
            {
                MyLog.Default.WriteLineAndConsole("ERROR in ToolbarItemChanged(): " + e);
            }
        }

        public void EquipItemAt(MyDefinitionId item, int slot, long identityId)
        {
            MyVisualScriptLogicProvider.ToolbarItemChanged -= ToolbarItemChanged;
            MyVisualScriptLogicProvider.SetToolbarSlotToItemLocal(slot, item, identityId);
            MyVisualScriptLogicProvider.ToolbarItemChanged += ToolbarItemChanged;
        }

        public static bool IsToolbarCharacter()
        {
            if (MyAPIGateway.Session.Player.Character == null)
                return false;
            return MyAPIGateway.Session.Player.Character.Parent == null && MyAPIGateway.Session.Player.Controller.ControlledEntity is IMyCharacter;
        }

        private bool IsEnabled()
        {
            return modOverrideEnabled && config.ModEnabled && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None && !MyAPIGateway.Gui.IsCursorVisible && !MyAPIGateway.Gui.ChatEntryVisible 
                && !MyAPIGateway.Session.IsCameraUserControlledSpectator && string.IsNullOrWhiteSpace(MyAPIGateway.Gui.ActiveGamePlayScreen) && (!BvApiClient.Registered || !BvApiClient.Open);
        }

        public static string GetKeyName(VRage.Input.MyKeys key)
        {
            if (key == VRage.Input.MyKeys.None)
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
                Tool t;
                ToolGroup tg;
                int index;
                if (config.ModEnabled && config.TryGetTool(id, out t, out index, out tg) && t.Enabled)
                {
                    if (tg.IsSlot(page, slot))
                    {
                        if (MyAPIGateway.Session.CreativeMode)
                            t.EquipIndex = index;

                        t.Equip();
                    }
                    else
                    {
                        if (MyAPIGateway.Session.CreativeMode)
                            t.EquipIndex = index;

                        t.ClearSlot();
                        t.Page = page;
                        t.Slot = slot;
                        if (t.Menu != null)
                            t.Menu.SlotUpdated();
                        config.ToolEdited(t, true);
                        config.Save();
                    }
                }
            }
        }
    }
}