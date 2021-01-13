using avaness.ToolSwitcherPlugin.Definitions;
using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.Game.Screens.Helpers;
using VRage.Game;

namespace avaness.ToolSwitcherPlugin
{
    public class PlayerToolbar
    {
        private readonly MyToolbar toolbar;
        private readonly ToolDefinitions defs;
        private int? activeToolPage;
        private readonly ToolSlot[] toolPages = new ToolSlot[9];

        public PlayerToolbar(MyToolbar toolbar, ToolDefinitions defs)
        {
            this.toolbar = toolbar;
            this.defs = defs;
            toolbar.SlotActivated += Toolbar_SlotActivated;
            toolbar.Unselected += Toolbar_Unselected;
            toolbar.ItemChanged += Toolbar_ItemChanged;

            for(int i = 0; i < 9 * 9; i++)
            {
                ToolSlot slot = CreateSlot(toolbar[i], i);
                if (slot != null)
                {
                    ToolSlot current = toolPages[slot.Page];
                    if (current != null)
                        current.Clear(toolbar);
                    toolPages[slot.Page] = slot;
                }
            }

            ToolSlot initialTool = toolPages[toolbar.CurrentPage];
            if (initialTool != null)
                activeToolPage = toolbar.CurrentPage;
        }

        private void Toolbar_ItemChanged(MyToolbar toolbar, MyToolbar.IndexArgs index, bool arg3)
        {
            ToolSlot slot = CreateSlot(toolbar.GetItemAtIndex(index.ItemIndex), index.ItemIndex);
            if (slot != null)
            {
                ToolSlot current = toolPages[slot.Page];
                if (current != null && current.Slot != slot.Slot)
                    current.Clear(toolbar);
                toolPages[slot.Page] = slot;
            }
        }

        public void Unload()
        {
            toolbar.SlotActivated -= Toolbar_SlotActivated;
            toolbar.Unselected -= Toolbar_Unselected;
            toolbar.ItemChanged -= Toolbar_ItemChanged;
        }



        private void Toolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs slot, bool arg3)
        {
            if (!slot.SlotNumber.HasValue)
                return;

            ToolSlot newSlot = CreateSlot(toolbar.GetSlotItem(slot.SlotNumber.Value), toolbar.CurrentPage, slot.SlotNumber.Value);
            if (newSlot != null)
            {
                ToolSlot current = toolPages[newSlot.Page];
                if (current == null || current.Slot != newSlot.Slot)
                    toolPages[newSlot.Page] = newSlot;
                activeToolPage = newSlot.Page;
            }
        }

        private void Toolbar_Unselected(MyToolbar toolbar)
        {
            activeToolPage = null;
        }

        public ToolSlot GetSelectedSlot()
        {
            if (!activeToolPage.HasValue)
                return null;

            int currentPage = toolbar.CurrentPage;
            ToolSlot currentSlot = toolPages[currentPage];
            if (currentSlot != null)
                return currentSlot;

            return toolPages[activeToolPage.Value];
        }

        private ToolSlot CreateSlot(MyToolbarItem item, int page, int slot)
        {
            MyToolbarItemDefinition itemDef = item as MyToolbarItemDefinition;
            if (itemDef?.Definition != null && defs.ContainsPhysical(itemDef.Definition.Id))
                return new ToolSlot(page, slot, itemDef);
            return null;
        }

        private ToolSlot CreateSlot(MyToolbarItem item, int index)
        {
            MyToolbarItemDefinition itemDef = item as MyToolbarItemDefinition;
            if (itemDef?.Definition != null && defs.ContainsPhysical(itemDef.Definition.Id))
                return new ToolSlot(index, itemDef);
            return null;
        }

        public void SetTool(MyDefinitionId physicalId)
        {
            ToolSlot refer = null;
            if(activeToolPage.HasValue)
            {
                refer = toolPages[activeToolPage.Value];
            }
            else
            {
                foreach(ToolSlot slot in toolPages)
                {
                    if(slot != null)
                    {
                        refer = slot;
                        break;
                    }
                }
            }

            if (refer == null)
                return;

            toolbar.Unselect(false);

            refer.SetTo(toolbar, physicalId);

            foreach(ToolSlot slot in toolPages)
            {
                if (slot != null && slot != refer)
                    slot.CopyFrom(toolbar, refer);
            }

            refer.Activate(toolbar);
            activeToolPage = refer.Page;
        }
    }
}
