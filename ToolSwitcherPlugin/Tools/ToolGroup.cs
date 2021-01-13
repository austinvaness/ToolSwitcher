using avaness.ToolSwitcherPlugin.Definitions;
using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Screens.Helpers;
using System.Collections.Generic;
using VRage.Game;
using System.Linq;
using Sandbox.ModAPI;

namespace avaness.ToolSwitcherPlugin.Tools
{
    public class ToolGroup
    {
        private readonly Tool[] tools;
        private readonly ToolDefinitions defs;

        public ToolGroup(ToolDefinitions defs)
        {
            this.defs = defs;
            tools = defs.Select((d) => new Tool(d)).ToArray();
        }

        public void EquipNext(PlayerCharacter ch, bool dir)
        {
            ToolSlot hand = ch.Toolbar.GetSelectedSlot();

            if (hand == null)
                return;

            for(int i = 0; i < tools.Length; i++)
            {
                if(tools[i].Contains(hand.PhysicalId))
                {
                    if(dir)
                    {
                        if (i < tools.Length - 1)
                            tools[i + 1].Equip(hand, ch);
                        else
                            tools[0].Equip(hand, ch);
                    }
                    else
                    {
                        if (i > 0)
                            tools[i - 1].Equip(hand, ch);
                        else
                            tools[tools.Length - 1].Equip(hand, ch);
                    }
                    return;
                }
            }
        }

        /*public void ReplaceItem(HandItem item, MyInventory inv, ToolSlot slot, MyToolbar toolbar)
        {
            for(int i = 0; i < tools.Length; i++)
            {
                if(tools[i].Contains(item))
                {
                    if (tools[i].Equip(item, inv, toolbar, slot))
                        return;

                    if (i < tools.Length - 1)
                        tools[i + 1].Equip(item, inv, toolbar, slot);
                    else
                        tools[0].Equip(item, inv, toolbar, slot);
                    return;
                }
            }
        }*/

        /*private ToolSlot GetSlot(MyToolbar toolbar)
        {
            int currPageStart = toolbar.SlotToIndex(0);

            if (toolbar.SelectedSlot.HasValue)
            {
                if (toolbar.SelectedItem is MyToolbarItemDefinition item && defs.ContainsPhysical(item.Definition.Id))
                    return new ToolSlot(toolbar.CurrentPage, toolbar.SelectedSlot.Value);
            }

            int currPageEnd = currPageStart + 8;

            ToolSlot result = GetSlot(toolbar, currPageStart, currPageEnd);
            if (result != null)
                return result;

            if (currPageStart > 0)
            {
                result = GetSlot(toolbar, currPageStart - 1, 0);
                if (result != null)
                    return result;
            }

            int len = toolbar.PageCount * toolbar.SlotCount;
            if (currPageEnd < len - 1)
            {
                result = GetSlot(toolbar, currPageEnd + 1, len - 1);
                if (result != null)
                    return result;
            }

            return ToolSlot.GetSlot(toolbar);
        }

        private ToolSlot GetSlot(MyToolbar toolbar, int start, int end)
        {
            MyToolbarItemDefinition toolbarItem;
            if (start > end)
            {
                for (int i = start; i >= end; i--)
                {
                    toolbarItem = toolbar[i] as MyToolbarItemDefinition;
                    if (toolbarItem != null && defs.ContainsPhysical(toolbarItem.Definition.Id))
                        return new ToolSlot(i);
                }
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    toolbarItem = toolbar[i] as MyToolbarItemDefinition;
                    if (toolbarItem != null && defs.ContainsPhysical(toolbarItem.Definition.Id))
                        return new ToolSlot(i);
                }
            }
            return null;
        }*/
    }
}
