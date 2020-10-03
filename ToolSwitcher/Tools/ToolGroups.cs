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
        private readonly HudAPIv2 hud;
        private HudAPIv2.MenuRootCategory hudCategory;
        private HudAPIv2.MenuKeybindInput equipInput;
        private readonly char[] space = new char[] { ' ' };
        private readonly IMyPlayer p = MyAPIGateway.Session.Player;

        private bool menuEnabled = true;
        public bool MenuEnabled
        {
            get
            {
                return menuEnabled;
            }
            set
            {
                if(hud.Heartbeat && menuEnabled != value)
                {
                    welderMenu.SetInteractable(value);
                    grinderMenu.SetInteractable(value);
                    drillMenu.SetInteractable(value);
                    rifleMenu.SetInteractable(value);
                    hudCategory.Interactable = value;
                    equipInput.Interactable = value;
                    menuEnabled = value;
                }
            }
        }

        public ToolGroups()
        {
            welder = new WelderTool(MyKeys.None, 0, 0);
            ToolEdited(welder, false);
            grinder = new GrinderTool(MyKeys.None, 1, 0);
            ToolEdited(grinder, false);
            drill = new DrillTool(MyKeys.None, 2, 0);
            ToolEdited(drill, false);
            rifle = new RifleTool(MyKeys.None, 3, 0);
            ToolEdited(rifle, false);
            hud = new HudAPIv2(OnHudReady);
        }

        public void Unload()
        {
            hud.Unload();
        }

        private void OnHudReady()
        {
            hudCategory = new HudAPIv2.MenuRootCategory("Tool Switcher", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Tool Switcher");
            welderMenu = new ToolMenu(hudCategory, welder, this);
            grinderMenu = new ToolMenu(hudCategory, grinder, this);
            drillMenu = new ToolMenu(hudCategory, drill, this);
            rifleMenu = new ToolMenu(hudCategory, rifle, this);
            equipInput = new HudAPIv2.MenuKeybindInput("Equip All Key - " + ToolSwitcherSession.GetKeyName(EquipAllKey), hudCategory, "Press any key.", OnEquipAllKeySubmit);
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
            if (!menuEnabled)
                return;

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

        public void ToolEdited(Tool tool, bool equip = true)
        {
            bool toolbar = ToolSwitcherSession.IsToolbarCharacter() && equip;
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                ToolGroup g = groups[i];
                if (g.Remove(tool))
                {
                    if (g.Count == 0)
                        groups.RemoveAtFast(i);
                    else if(toolbar)
                        g.First().Equip();
                }
            }
            
            for (int i = 0; i < groups.Count; i++)
            {
                ToolGroup g = groups[i];
                if (g.ShouldContain(tool))
                {
                    g.Add(tool);
                    if (toolbar)
                        tool.Equip();
                    return;
                }
            }

            groups.Add(new ToolGroup(tool.Page, tool.Slot, tool));
            if(toolbar)
                tool.Equip();
        }

        public IEnumerator<ToolGroup> GetEnumerator()
        {
            return groups.GetEnumerator();
        }
    }
}
