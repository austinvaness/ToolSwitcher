using avaness.ToolSwitcher.Tools;
using Draygo.API;
using VRage.Input;

namespace avaness.ToolSwitcher
{
    public class ToolMenu
    {
        private readonly HudAPIv2.MenuSubCategory toolCategory;
        private readonly HudAPIv2.MenuKeybindInput keyInput;
        private readonly HudAPIv2.MenuTextInput slotInput, pageInput;
        private readonly HudAPIv2.MenuItem btnEnabled;
        private readonly ToolGroups config;
        private bool interactable = true;
        private Tool tool;

        public ToolMenu(HudAPIv2.MenuCategoryBase category, Tool tool, ToolGroups config)
        {
            this.tool = tool;
            this.config = config;
            toolCategory = new HudAPIv2.MenuSubCategory("", category, tool.Name);
            SetCategoryText();
            keyInput = new HudAPIv2.MenuKeybindInput("Key - " + ToolSwitcherSession.GetKeyName(tool.Keybind), toolCategory, "Press any key.", OnKeySubmit);
            slotInput = new HudAPIv2.MenuTextInput("Slot - " + (tool.Slot + 1), toolCategory, "Enter a slot number 1-9.", OnSlotSubmit);
            pageInput = new HudAPIv2.MenuTextInput("Page - " + (tool.Page + 1), toolCategory, "Enter a page number 1-9.", OnPageSubmit);
            btnEnabled = new HudAPIv2.MenuItem("Enabled - " + tool.Enabled, toolCategory, OnEnabledSubmit);
        }

        public void SetTool(Tool tool)
        {
            this.tool = tool;
            SetCategoryText();
            keyInput.Text = "Key - " + ToolSwitcherSession.GetKeyName(tool.Keybind);
            slotInput.Text = "Slot - " + (tool.Slot + 1);
            pageInput.Text = "Page - " + (tool.Page + 1);
            btnEnabled.Text = "Enabled - " + tool.Enabled;
        }

        public void SetInteractable(bool interactable)
        {
            toolCategory.Interactable = interactable;
            keyInput.Interactable = interactable;
            slotInput.Interactable = interactable;
            pageInput.Interactable = interactable;
            btnEnabled.Interactable = interactable;
            this.interactable = interactable;
        }

        private void OnEnabledSubmit()
        {
            if (!interactable)
                return;

            tool.Enabled = !tool.Enabled;
            btnEnabled.Text = "Enabled - " + tool.Enabled;
            config.Save();
        }

        private void OnSlotSubmit(string s)
        {
            if (!interactable)
                return;

            int slot;
            s = s.Trim();
            if(s.Length == 1 && int.TryParse(s, out slot) && slot >= 1 && slot <= 9)
            {
                tool.ClearSlot();
                tool.Slot = slot - 1;
                config.ToolEdited(tool);
                config.Save();
                slotInput.Text = "Slot - " + s;
                SetCategoryText();
            }
        }

        private void SetCategoryText()
        {
            toolCategory.Text = $"{tool.Name}  ({tool.Page + 1}, {tool.Slot + 1})";
        }

        public void SlotUpdated()
        {
            SetCategoryText();
            slotInput.Text = "Slot - " + (tool.Slot + 1);
            pageInput.Text = "Page - " + (tool.Page + 1);
        }

        private void OnPageSubmit(string s)
        {
            if (!interactable)
                return;

            int page;
            s = s.Trim();
            if(s.Length == 1 && int.TryParse(s, out page) && page >= 1 && page <= 9)
            {
                tool.ClearSlot();
                tool.Page = page - 1;
                config.ToolEdited(tool);
                config.Save();
                pageInput.Text = "Page - " + s;
                SetCategoryText();
            }
        }

        private void OnKeySubmit(MyKeys key, bool shift, bool ctrl, bool alt)
        {
            if (!interactable)
                return;

            tool.Keybind = key;
            config.Save();
            keyInput.Text = "Key - " + ToolSwitcherSession.GetKeyName(key);
        }
    }
}
