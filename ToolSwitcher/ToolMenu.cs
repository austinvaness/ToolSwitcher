using avaness.ToolSwitcher.Tools;
using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Input;
using VRage.ModAPI;

namespace avaness.ToolSwitcher
{
    public class ToolMenu
    {
        private readonly HudAPIv2.MenuSubCategory toolCategory;
        private readonly HudAPIv2.MenuKeybindInput keyInput;
        private readonly HudAPIv2.MenuTextInput slotInput, pageInput;
        private readonly HudAPIv2.MenuItem btnEnabled;
        private readonly Tool tool;
        private readonly ToolGroups config;

        public ToolMenu(HudAPIv2.MenuCategoryBase category, Tool tool, ToolGroups config)
        {
            this.tool = tool;
            this.config = config;
            toolCategory = new HudAPIv2.MenuSubCategory(tool.Name, category, tool.Name);
            keyInput = new HudAPIv2.MenuKeybindInput("Key - " + ToolSwitcherSession.GetKeyName(tool.Keybind), toolCategory, "Press any key.", OnKeySubmit);
            slotInput = new HudAPIv2.MenuTextInput("Slot - " + (tool.Slot + 1), toolCategory, "Enter a slot number 1-9.", OnSlotSubmit);
            pageInput = new HudAPIv2.MenuTextInput("Page - " + (tool.Page + 1), toolCategory, "Enter a page number 1-9.", OnPageSubmit);
            btnEnabled = new HudAPIv2.MenuItem("Enabled - " + tool.Enabled, toolCategory, OnEnabledSubmit);
        }

        private void OnEnabledSubmit()
        {
            tool.Enabled = !tool.Enabled;
            btnEnabled.Text = "Enabled - " + tool.Enabled;
        }

        private void OnSlotSubmit(string s)
        {
            int slot;
            s = s.Trim();
            if(s.Length == 1 && int.TryParse(s, out slot) && slot >= 1 && slot <= 9)
            {
                tool.ClearSlot();
                tool.Slot = slot - 1;
                config.ToolEdited(tool);
                config.Save();
                slotInput.Text = "Slot - " + s;
            }
        }

        private void OnPageSubmit(string s)
        {
            int page;
            s = s.Trim();
            if(s.Length == 1 && int.TryParse(s, out page) && page >= 1 && page <= 9)
            {
                tool.ClearSlot();
                tool.Page = page - 1;
                config.ToolEdited(tool);
                config.Save();
                pageInput.Text = "Page - " + s;
            }
        }

        private void OnKeySubmit(MyKeys key, bool shift, bool ctrl, bool alt)
        {
            tool.Keybind = key;
            config.ToolEdited(tool);
            config.Save();
            keyInput.Text = "Key - " + ToolSwitcherSession.GetKeyName(key);
        }
    }
}
