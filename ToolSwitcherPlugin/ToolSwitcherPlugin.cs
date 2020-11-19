﻿using Sandbox.Game.World;
using System;
using VRage.Plugins;
using VRage.Utils;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using Sandbox.ModAPI;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Screens.Helpers;
using avaness.ToolSwitcherPlugin.Tools;
using Sandbox.Definitions;
using Sandbox.Common.ObjectBuilders.Definitions;
using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.ModAPI.Weapons;

namespace avaness.ToolSwitcherPlugin
{
    public class ToolSwitcherPlugin : IPlugin, IDisposable
    {
        [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
        public class ToolPluginSession : MySessionComponentBase
        {
            private bool start;
            private Tool<MyObjectBuilder_WelderDefinition> welder;
            private Tool<MyObjectBuilder_AngleGrinderDefinition> grinder;
            private Tool<MyObjectBuilder_HandDrillDefinition> drill;
            private ToolGroup group;

            public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
            {
                MyLog.Default.WriteLineAndConsole("Tool Plugin Session loaded.");
            }

            protected override void UnloadData()
            {

            }

            public override void UpdateAfterSimulation()
            {
                if (MySession.Static?.LocalCharacter == null || MyAPIGateway.Input.DeltaMouseScrollWheelValue() == 0)
                    return;

                if (!start)
                    Start();

                if(IsEnabled())
                {
                    MyCharacter ch = MySession.Static.LocalCharacter;

                    var hand = GetHand(ch);
                    if (hand != null)
                    {
                        if (ch.ToolbarType == MyToolbarType.Character)
                        {
                            MyToolbar toolbar = ch.Toolbar;
                            if (toolbar != null)
                                group.EquipNext(hand, ch.GetInventory(), toolbar, MyAPIGateway.Input.DeltaMouseScrollWheelValue() > 0);
                        }
                    }
                }

            }

            private void Start()
            {
                welder = new Tool<MyObjectBuilder_WelderDefinition>();
                grinder = new Tool<MyObjectBuilder_AngleGrinderDefinition>();
                drill = new Tool<MyObjectBuilder_HandDrillDefinition>();
                group = new ToolGroup(welder, grinder, drill);
                start = true;
            }

            private bool IsEnabled()
            {
                return  MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None && !MyAPIGateway.Gui.IsCursorVisible && !MyAPIGateway.Gui.ChatEntryVisible
                    && !MyAPIGateway.Session.IsCameraUserControlledSpectator && string.IsNullOrWhiteSpace(MyAPIGateway.Gui.ActiveGamePlayScreen);
            }

            private HandItem GetHand(MyCharacter ch)
            {
                var temp = ch.EquippedTool as IMyHandheldGunObject<MyToolBase>;
                if (temp != null && !(temp is IMyBlockPlacerBase))
                    return new HandItem(temp);
                return null;
            }

        }

        public void Dispose()
        {

        }

        public void Init(object gameInstance)
        {

        }

        public void Update()
        {
        }
    }
}
