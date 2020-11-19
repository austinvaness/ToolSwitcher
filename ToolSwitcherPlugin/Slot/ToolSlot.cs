using avaness.ToolSwitcherPlugin.Tools;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace avaness.ToolSwitcherPlugin.Slot
{
    public class ToolSlot
    {
        private readonly int page;
        private readonly int slot;

        public ToolSlot(int page, int slot)
        {
            this.page = page;
            this.slot = slot;
        }

        public void SetTo(MyToolbar toolbar, MyDefinitionId physicalId)
        {
            toolbar.SwitchToPage(page);
            MyVisualScriptLogicProvider.SetToolbarSlotToItemLocal(slot, physicalId);
            toolbar.ActivateItemAtSlot(slot);
        }

        public static ToolSlot GetSlot(MyToolbar toolbar)
        {
            int currPageStart = toolbar.SlotToIndex(0);
            for(int i = currPageStart; i <= currPageStart + 8; i++)
            {
                var item = toolbar.GetItemAtIndex(i);
                if (item == null || !(item is MyToolbarItemDefinition))
                    return new ToolSlot(toolbar.CurrentPage, i - currPageStart);
            }
            return new ToolSlot(toolbar.CurrentPage, toolbar.SelectedSlot ?? 0);
        }

    }

}
