using System;
using System.Collections.Generic;
using System.Linq;
using avaness.ToolSwitcher.Tools;
using Sandbox.Game;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using VRage;
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
        }

        private void Start()
        {
            init = true;
            config = ToolGroups.Load();
            equipAll = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session?.Player == null)
                return;
            if (!init)
                Start();

            if(IsEnabled())
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

                //config.Debug();
            }
        }

        private bool IsEnabled()
        {
            return MyAPIGateway.Session.Player.Character != null && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None
                    && !MyAPIGateway.Gui.IsCursorVisible && !MyAPIGateway.Gui.ChatEntryVisible && !MyAPIGateway.Session.IsCameraUserControlledSpectator;
        }

        public static string GetKeyName(MyKeys key)
        {
            if (key == MyKeys.None)
                return "None";
            return MyAPIGateway.Input.GetKeyName(key);
        }
    }
}