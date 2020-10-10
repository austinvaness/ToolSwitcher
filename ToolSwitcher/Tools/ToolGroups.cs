using Draygo.API;
using Sandbox.Definitions;
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
using VRage;
using VRage.Game;
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

        private bool menuEnabled = true;
        [XmlIgnore]
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
                    welder.Menu.SetInteractable(value);
                    grinder.Menu.SetInteractable(value);
                    drill.Menu.SetInteractable(value);
                    rifle.Menu.SetInteractable(value);
                    hudCategory.Interactable = value;
                    equipInput.Interactable = value;
                    foreach (ModTool t in modTools)
                        t.Menu.SetInteractable(value);
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

            foreach(MyHandItemDefinition def in MyDefinitionManager.Static.GetHandItemDefinitions())
            {
                if(!def.Context.IsBaseGame)
                {
                    ModTool modTool = new ModTool(MyKeys.None, 0, 0, def.PhysicalItemId);
                    modTools.Add(modTool);
                    ToolEdited(modTool, false);
                }
            }
            serializableTools = modTools.ToArray();

            hud = new HudAPIv2(OnHudReady);
        }

        public void Unload()
        {
            hud.Unload();
        }

        private void OnHudReady()
        {
            hudCategory = new HudAPIv2.MenuRootCategory("Tool Switcher", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Tool Switcher");
            welder.Menu = new ToolMenu(hudCategory, welder, this);
            grinder.Menu = new ToolMenu(hudCategory, grinder, this);
            drill.Menu = new ToolMenu(hudCategory, drill, this);
            rifle.Menu = new ToolMenu(hudCategory, rifle, this);

            for(int i = 0; i < modTools.Count; i++)
            {
                ModTool t = modTools[i];
                t.Menu = new ToolMenu(hudCategory, t, this);
            }

            equipInput = new HudAPIv2.MenuKeybindInput("Equip All Key - " + ToolSwitcherSession.GetKeyName(EquipAllKey), hudCategory, "Press any key.", OnEquipAllKeySubmit);
        }

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
                ToolMenu menu = grinder.Menu;
                grinder = value;
                if (menu != null)
                {
                    menu.SetTool(grinder);
                    grinder.Menu = menu;
                }
                ToolEdited(grinder);
            }
        }

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
                ToolMenu menu = welder.Menu;
                welder = value;
                if (menu != null)
                {
                    menu.SetTool(welder);
                    welder.Menu = menu;
                }
                ToolEdited(welder);
            }
        }

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
                ToolMenu menu = drill.Menu;
                drill = value;
                if (menu != null)
                {
                    menu.SetTool(drill);
                    drill.Menu = menu;
                }
                ToolEdited(drill);
            }
        }

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
                ToolMenu menu = rifle.Menu;
                rifle = value;
                if (menu != null)
                {
                    menu.SetTool(rifle);
                    rifle.Menu = menu;
                }
                ToolEdited(rifle);
            }
        }

        private List<ModTool> modTools = new List<ModTool>();
        private ModTool[] serializableTools = new ModTool[0];
        [XmlArray("ModTools")]
        public ModTool[] ModTools
        {
            get
            {
                return serializableTools;
            }
            set
            {
                Dictionary<string, Tuple<int, ModTool>> modTools = new Dictionary<string, Tuple<int, ModTool>>();
                for(int i = 0; i < this.modTools.Count; i++)
                {
                    ModTool t = this.modTools[i];
                    modTools.Add(t.Name, new Tuple<int, ModTool>(i, t));
                }

                foreach(ModTool t in value)
                {
                    if (t.Id.TypeId.IsNull)
                        continue;

                    Tuple<int, ModTool> tuple;
                    if(modTools.TryGetValue(t.Name, out tuple))
                    {
                        ToolMenu menu = tuple.Value2.Menu;
                        if (menu != null)
                        {
                            menu.SetTool(t);
                            t.Menu = menu;
                        }
                        this.modTools[tuple.Value1] = t;
                        ToolEdited(t);
                        tuple.Value2 = t;
                        modTools[t.Name] = tuple;
                    }
                    else
                    {
                        modTools[t.Name] = new Tuple<int, ModTool>(-1, t);
                    }
                }
                serializableTools = modTools.Values.Select(t => t.Value2).ToArray();
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
                    config.Save();
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


        private class Tuple<T1, T2>
        {
            public T1 Value1;
            public T2 Value2;
            public Tuple() { }
            public Tuple(T1 val1, T2 val2)
            {
                Value1 = val1;
                Value2 = val2;
            }
        }
    }
}
