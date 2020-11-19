using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Screens.Helpers;
using System.Collections.Generic;
using VRage.Game;

namespace avaness.ToolSwitcherPlugin.Tools
{
    public class ToolGroup
    {
        private readonly ITool[] tools;
        private readonly HashSet<MyDefinitionId> physicalIds;

        public ToolGroup(params ITool[] tools)
        {
            this.tools = tools;
            physicalIds = new HashSet<MyDefinitionId>();
            foreach(ITool tool in tools)
            {
                foreach (MyEngineerToolBaseDefinition def in tool.Defs)
                    physicalIds.Add(def.PhysicalItemId);
            }
        }

        public void EquipNext(HandItem hand, MyInventory inv, MyToolbar toolbar, bool dir)
        {
            for(int i = 0; i < tools.Length; i++)
            {
                if(tools[i].Contains(hand))
                {
                    ToolSlot slot = GetSlot(toolbar);
                    if(dir)
                    {
                        if (i < tools.Length - 1)
                            tools[i + 1].Equip(hand, inv, toolbar, slot);
                        else
                            tools[0].Equip(hand, inv, toolbar, slot);
                    }
                    else
                    {
                        if (i > 0)
                            tools[i - 1].Equip(hand, inv, toolbar, slot);
                        else
                            tools[tools.Length - 1].Equip(hand, inv, toolbar, slot);
                    }
                    return;
                }
            }
        }

        private ToolSlot GetSlot(MyToolbar toolbar)
        {
            int currPageStart = toolbar.SlotToIndex(0);

            if (toolbar.SelectedSlot.HasValue)
            {
                if (toolbar.SelectedItem is MyToolbarItemDefinition item && physicalIds.Contains(item.Definition.Id))
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
                    if (toolbarItem != null && physicalIds.Contains(toolbarItem.Definition.Id))
                        return new ToolSlot(i / 9, i % 9);
                }
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    toolbarItem = toolbar[i] as MyToolbarItemDefinition;
                    if (toolbarItem != null && physicalIds.Contains(toolbarItem.Definition.Id))
                        return new ToolSlot(i / 9, i % 9);
                }
            }
            return null;
        }
    }
}
