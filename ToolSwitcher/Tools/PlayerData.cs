using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using VRage.Game.ModAPI;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    [Serializable]
    public class PlayerData
    {
        private const string fileName = "ToolSwitcher-Settings.xml";
        private readonly List<ToolGroup> groups = new List<ToolGroup>();
        private HudAPIv2 hud;
        private HudAPIv2.MenuRootCategory hudCategory;
        private readonly IMyPlayer p = MyAPIGateway.Session.Player;

        public PlayerData()
        {
            grinder = new GrinderTool(MyKeys.None, 0, 0);
            welder = new WelderTool(MyKeys.None, 0, 0);
            drill = new DrillTool(MyKeys.None, 0, 0);
            hud = new HudAPIv2(OnHudReady);
        }

        public void Unload()
        {
            hud.Unload();
        }

        private void OnHudReady()
        {
            hudCategory = new HudAPIv2.MenuRootCategory("Tool Switcher", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Tool Switcher");
            grinderMenu = new ToolMenu(hudCategory, grinder, this);
            welderMenu = new ToolMenu(hudCategory, welder, this);
            drillMenu = new ToolMenu(hudCategory, drill, this);
        }

        public void Debug()
        {
            MyAPIGateway.Utilities.ShowNotification($"{groups.Count} total groups.", 16);
            foreach (ToolGroup g in groups)
                g.Debug();
        }

        private ToolMenu grinderMenu;
        private GrinderTool grinder;
        [XmlElement]
        public GrinderTool Grinder
        {
            get
            {
                return grinder;
            }
            set
            {
                grinder = value;
                ToolEdited(grinder);
            }
        }

        private ToolMenu welderMenu;
        private WelderTool welder;
        [XmlElement]
        public WelderTool Welder
        {
            get
            {
                return welder;
            }
            set
            {
                welder = value;
                ToolEdited(welder);
            }
        }

        private ToolMenu drillMenu;
        private DrillTool drill;
        [XmlElement]
        public DrillTool Drill
        {
            get
            {
                return drill;
            }
            set
            {
                drill = value;
                ToolEdited(drill);
            }
        }

        public static PlayerData Load()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(fileName))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(fileName);
                    string xmlText = reader.ReadToEnd();
                    reader.Close();
                    PlayerData config = MyAPIGateway.Utilities.SerializeFromXML<PlayerData>(xmlText);
                    if (config == null)
                        throw new NullReferenceException("Failed to serialize from xml.");
                    else
                        return config;
                }
            }
            catch { }

            PlayerData result = new PlayerData();
            result.Save();
            return result;
        }

        public void Save()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(fileName);
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(this));
            writer.Flush();
            writer.Close();
        }

        public void ToolEdited(Tool tool)
        {
            bool newGroup = true;
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                ToolGroup g = groups[i];
                if (g.Remove(tool))
                {
                    if (g.Count == 0)
                        groups.RemoveAtFast(i);
                }
                else if (g.ShouldContain(tool))
                {
                    g.Add(tool);
                    newGroup = false;
                }
            }
            if (newGroup)
                groups.Add(new ToolGroup(tool.Page, tool.Slot, tool));
        }

        public IEnumerator<ToolGroup> GetEnumerator()
        {
            return groups.GetEnumerator();
        }
    }
}
