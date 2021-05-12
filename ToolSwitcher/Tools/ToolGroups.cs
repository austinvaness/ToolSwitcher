using Draygo.API;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VRage.Game;
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
        private HudAPIv2.MenuSubCategory toolsCat;
        private HudAPIv2.MenuSubCategory vanillaToolCat;
        private List<HudAPIv2.MenuSubCategory> modToolCats;
        private HudAPIv2.MenuKeybindInput equipInput;
        private HudAPIv2.MenuKeybindInput upgradeInput;
        private HudAPIv2.MenuKeybindInput downgradeInput;
        private HudAPIv2.MenuItem modEnabledInput;
        private bool loaded = false;

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
                    hudCategory.Interactable = value;
                    welder.Menu.SetInteractable(value);
                    grinder.Menu.SetInteractable(value);
                    drill.Menu.SetInteractable(value);
                    rifle.Menu.SetInteractable(value);
                    pistol.Menu.SetInteractable(value);
                    launcher.Menu.SetInteractable(value);
                    hudCategory.Interactable = value;
                    equipInput.Interactable = value;
                    upgradeInput.Interactable = value;
                    downgradeInput.Interactable = value;
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
            pistol = new PistolTool(MyKeys.None, 3, 0);
            ToolEdited(pistol, false);
            launcher = new LauncherTool(MyKeys.None, 3, 0);
            ToolEdited(launcher, false);
            rifle = new RifleTool(MyKeys.None, 3, 0);
            ToolEdited(rifle, false);


            foreach (MyHandItemDefinition def in MyDefinitionManager.Static.GetHandItemDefinitions())
            {
                if(!def.Context.IsBaseGame)
                {
                    string modName = def.Context.ModName;
                    if (string.IsNullOrWhiteSpace(modName))
                    {
                        if (string.IsNullOrWhiteSpace(def.Context.ModId))
                            modName = null;
                        else
                            modName = def.Context.ModId.Trim();
                    }
                    else
                    {
                        modName = modName.Trim();
                    }

                    if (modName != null && modName.Length > 30)
                        modName = modName.Substring(0, 30);

                    ModTool modTool = new ModTool(MyKeys.None, 0, 0, def.PhysicalItemId, modName);
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
            groups.Clear();
        }

        private void OnHudReady()
        {
            hudCategory = new HudAPIv2.MenuRootCategory("Tool Switcher", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Tool Switcher");
            modEnabledInput = new HudAPIv2.MenuItem("Mod Enabled - " + ModEnabled, hudCategory, OnModEnabledSubmit);

            toolsCat = new HudAPIv2.MenuSubCategory("Tools", hudCategory, "Tools");

            if (modTools.Count == 0)
                vanillaToolCat = toolsCat;
            else
                vanillaToolCat = new HudAPIv2.MenuSubCategory("Vanilla", toolsCat, "Vanilla Tools");
            welder.Menu = new ToolMenu(vanillaToolCat, welder, this);
            grinder.Menu = new ToolMenu(vanillaToolCat, grinder, this);
            drill.Menu = new ToolMenu(vanillaToolCat, drill, this);
            rifle.Menu = new ToolMenu(vanillaToolCat, rifle, this);
            pistol.Menu = new ToolMenu(vanillaToolCat, pistol, this);
            launcher.Menu = new ToolMenu(vanillaToolCat, launcher, this);

            HudAPIv2.MenuSubCategory unknownCategory = null;
            Dictionary<string, HudAPIv2.MenuSubCategory> modToolCats = new Dictionary<string, HudAPIv2.MenuSubCategory>();
            for (int i = 0; i < modTools.Count; i++)
            {
                ModTool t = modTools[i];
                HudAPIv2.MenuCategoryBase c;
                if (t.ModName == null)
                {
                    if (unknownCategory == null)
                        unknownCategory = new HudAPIv2.MenuSubCategory("Mod: Unknown", toolsCat, "Unknown");
                    c = unknownCategory;
                }
                else
                {
                    HudAPIv2.MenuSubCategory subC;
                    if (!modToolCats.TryGetValue(t.ModName, out subC))
                    {
                        subC = new HudAPIv2.MenuSubCategory("Mod: " + t.ModName, toolsCat, t.ModName);
                        modToolCats[t.ModName] = subC;
                    }
                    c = subC;
                }
                t.Menu = new ToolMenu(c, t, this);
            }
            this.modToolCats = modToolCats.Values.ToList();

            equipInput = new HudAPIv2.MenuKeybindInput("Equip All Key - " + ToolSwitcherSession.GetKeyName(EquipAllKey), hudCategory, "Press any key.", OnEquipAllKeySubmit);
            upgradeInput = new HudAPIv2.MenuKeybindInput("Upgrade Key - " + ToolSwitcherSession.GetKeyName(UpgradeKey), hudCategory, "Press any key.", OnUpgradeKeySubmit);
            downgradeInput = new HudAPIv2.MenuKeybindInput("Downgrade Key - " + ToolSwitcherSession.GetKeyName(DowngradeKey), hudCategory, "Press any key.", OnDowngradeKeySubmit);
        }


        [XmlElement]
        public bool ModEnabled { get; set; } = true;

        private void OnModEnabledSubmit()
        {
            ModEnabled = !ModEnabled;
            modEnabledInput.Text = "Mod Enabled - " + ModEnabled;
            Save();
            ToolSwitcherSession.Instance.EquipAll();
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

        private PistolTool pistol;
        [XmlElement]
        public PistolTool Pistol
        {
            get
            {
                return pistol;
            }
            set
            {
                ToolMenu menu = pistol.Menu;
                pistol = value;
                if (menu != null)
                {
                    menu.SetTool(pistol);
                    pistol.Menu = menu;
                }
                ToolEdited(pistol);
            }
        }

        private LauncherTool launcher;
        [XmlElement]
        public LauncherTool Launcher
        {
            get
            {
                return launcher;
            }
            set
            {
                ToolMenu menu = launcher.Menu;
                launcher = value;
                if (menu != null)
                {
                    menu.SetTool(launcher);
                    launcher.Menu = menu;
                }
                ToolEdited(launcher);
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
                        t.ModName = tuple.Value2.ModName;
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

        [XmlElement]
        public MyKeys UpgradeKey { get; set; } = MyKeys.None;

        private void OnUpgradeKeySubmit(MyKeys key, bool shift, bool ctrl, bool alt)
        {
            if (!menuEnabled)
                return;

            UpgradeKey = key;
            upgradeInput.Text = "Upgrade Key - " + ToolSwitcherSession.GetKeyName(key);
            Save();
        }

        [XmlElement]
        public MyKeys DowngradeKey { get; set; } = MyKeys.None;

        private void OnDowngradeKeySubmit(MyKeys key, bool shift, bool ctrl, bool alt)
        {
            if (!menuEnabled)
                return;

            DowngradeKey = key;
            downgradeInput.Text = "Downgrade Key - " + ToolSwitcherSession.GetKeyName(key);
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
                    config.loaded = true;
                    config.Save();
                    return config;
                }
            }
            catch { }

            ToolGroups result = new ToolGroups();
            result.loaded = true;
            result.Save();
            return result;
        }

        public void Save()
        {
            string data = MyAPIGateway.Utilities.SerializeToXML(this);
            MyAPIGateway.Parallel.StartBackground(() => SaveFile(data));
        }

        private static void SaveFile(string data)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(fileName);
                writer.Write(data);
                writer.Flush();
                writer.Close();
            }
            catch
            { }
        }

        public void ToolEdited(Tool tool, bool? equip = null)
        {
            bool toolbar;
            if (equip.HasValue)
                toolbar = equip.Value;
            else
                toolbar = ToolSwitcherSession.IsToolbarCharacter() && ModEnabled && loaded;

            for (int i = groups.Count - 1; i >= 0; i--)
            {
                ToolGroup g = groups[i];
                if (g.Remove(tool))
                {
                    if (g.Count == 0)
                        groups.RemoveAtFast(i);
                    else if (toolbar)
                        g.EquipAny();
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

        public bool TryGetTool(out Tool tool, out ToolGroup toolGroup)
        {
            var hand = MyAPIGateway.Session.Player.Character.EquippedTool as IMyHandheldGunObject<MyDeviceBase>;
            if (hand == null)
            {
                tool = null;
                toolGroup = null;
                return false;
            }
            int discard;
            return TryGetTool(hand.PhysicalItemDefinition.Id, out tool, out discard, out toolGroup);
        }

        public bool TryGetTool(MyDefinitionId id, out Tool tool, out int index, out ToolGroup toolGroup)
        {
            tool = null;
            toolGroup = null;
            foreach (ToolGroup g in groups)
            {
                Tool t;
                if (g.TryGetTool(id, out t, out index))
                {
                    tool = t;
                    toolGroup = g;
                    return true;
                }
            }
            index = -1;
            return false;
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
