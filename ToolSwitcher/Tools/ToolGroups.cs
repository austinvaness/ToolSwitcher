using Draygo.API;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.Utils;

namespace avaness.ToolSwitcher.Tools
{
    [Serializable]
    public class ToolGroups
    {
        private const string fileName = "ToolSwitcher-Settings.xml";
        private readonly List<ToolGroup> groups = new List<ToolGroup>();
        private HudAPIv2 hud;
        private HudAPIv2.MenuRootCategory hudCategory;
        private HudAPIv2.MenuKeybindInput equipInput;
        private readonly char[] space = new char[] { ' ' };
        private readonly IMyPlayer p = MyAPIGateway.Session.Player;

        public ToolGroups()
        {
            grinder = new GrinderTool(MyKeys.None, 0, 0);
            ToolEdited(grinder);
            welder = new WelderTool(MyKeys.None, 0, 0);
            ToolEdited(welder);
            drill = new DrillTool(MyKeys.None, 0, 0);
            ToolEdited(drill);
            rifle = new RifleTool(MyKeys.None, 0, 0);
            ToolEdited(rifle);
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
            rifleMenu = new ToolMenu(hudCategory, rifle, this);
            equipInput = new HudAPIv2.MenuKeybindInput("Equip All Key - " + ToolSwitcherSession.GetKeyName(EquipAllKey), hudCategory, "Press any key.", OnEquipAllKeySubmit);
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

        private ToolMenu rifleMenu;
        private RifleTool rifle;
        [XmlElement]
        public RifleTool Rifle
        {
            get
            {
                return rifle;
            }
            set
            {
                rifle = value;
                ToolEdited(rifle);
            }
        }

        [XmlElement]
        public MyKeys EquipAllKey { get; set; } = MyKeys.None;

        private void OnEquipAllKeySubmit(MyKeys key, bool shift, bool ctrl, bool alt)
        {
            EquipAllKey = key;
            equipInput.Text = "Equip All Key - " + ToolSwitcherSession.GetKeyName(key);
            Save();
        }

        public static ToolGroups Load()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(fileName))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(fileName);
                    string xmlText = reader.ReadToEnd();
                    reader.Close();
                    ToolGroups config = MyAPIGateway.Utilities.SerializeFromXML<ToolGroups>(xmlText);
                    if (config == null)
                        throw new NullReferenceException("Failed to serialize from xml.");
                    return config;
                }
            }
            catch { }

            ToolGroups result = new ToolGroups();
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
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                ToolGroup g = groups[i];
                if (g.Remove(tool))
                {
                    if (g.Count == 0)
                        groups.RemoveAtFast(i);
                }
            }
            
            for (int i = 0; i < groups.Count; i++)
            {
                ToolGroup g = groups[i];
                if (g.ShouldContain(tool))
                {
                    g.Add(tool);
                    return;
                }
            }

            groups.Add(new ToolGroup(tool.Page, tool.Slot, tool));
        }

        public IEnumerator<ToolGroup> GetEnumerator()
        {
            return groups.GetEnumerator();
        }
    }
}
