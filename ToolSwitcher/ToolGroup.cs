using avaness.ToolSwitcher.Tools;
using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace avaness.ToolSwitcher
{
    public class ToolGroup : IEnumerable<Tool>, IEquatable<ToolGroup>
    {
        private readonly List<Tool> tools;
        private readonly int slot;
        private readonly int page;
        private readonly IMyPlayer p = MyAPIGateway.Session.Player;

        public int Count => tools.Count;

        public ToolGroup(int page, int slot, params Tool[] tools)
        {
            this.page = page;
            this.slot = slot;
            this.tools = new List<Tool>(tools);
        }

        public bool Add(Tool tool)
        {
            if (tools.Contains(tool))
                return true;
            tools.Add(tool);
            return true;
        }

        public bool Remove(Tool tool)
        {
            int index = tools.IndexOf(tool);
            if(index >= 0)
            {
                tools.RemoveAtFast(index);
                return true;
            }
            return false;
        }

        public bool ShouldContain(Tool tool)
        {
            return tool.Page == page && tool.Slot == slot;
        }

        public bool Contains(Tool tool)
        {
            return tools.Contains(tool);
        }

        public bool TryGetTool(MyDefinitionId toolId, out Tool tool, out int index)
        {
            foreach (Tool t in tools)
            {
                if (t.HasId(toolId, out index))
                {
                    tool = t;
                    return true;
                }
            }
            tool = null;
            index = -1;
            return false;
        }

        public bool IsSlot(int page, int slot)
        {
            return this.page == page && this.slot == slot;
        }

        public IEnumerator<Tool> GetEnumerator()
        {
            return tools.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tools.GetEnumerator();
        }

        public void HandleInput()
        {
            if (tools.Count == 0 || p.Character == null)
                return;

            IMyInput input = MyAPIGateway.Input;
            if (tools.Count > 1 && !input.IsAnyCtrlKeyPressed() && !input.IsAnyAltKeyPressed())
            {
                int scroll = ReadScroll();
                if (scroll > 0)
                {
                    int visible = FindVisibleTool();
                    if (visible >= 0)
                    {
                        for (int i = LoopInc(visible); i != visible; i = LoopInc(i))
                        {
                            if (tools[i].Equip())
                                break;
                        }
                    }
                }
                else if (scroll < 0)
                {
                    int visible = FindVisibleTool();
                    if (visible >= 0)
                    {
                        for (int i = LoopDec(visible); i != visible; i = LoopDec(i))
                        {
                            if (tools[i].Equip())
                                break;
                        }
                    }
                }
            }

            for(int i = 0; i < tools.Count; i++)
            {
                Tool tool = tools[i];
                tool.HandleInput();
            }
        }

        private int FindVisibleTool()
        {
            for (int i = 0; i < tools.Count; i++)
            {
                if (tools[i].IsInHand())
                    return i;
            }
            return -1;
        }

        private int LoopInc(int i)
        {
            if (i >= tools.Count - 1)
                return 0;
            return i + 1;
        }

        private int LoopDec(int i)
        {
            if (i <= 0)
                return tools.Count - 1;
            return i - 1;
        }


        public int ReadScroll()
        {
            return MyAPIGateway.Input.DeltaMouseScrollWheelValue();
        }

        public bool EquipAny(MyDefinitionId? itemRemoved = null)
        {
            foreach(Tool t in tools)
            {
                if (t.Equip(itemRemoved))
                    return true;
            }
            return false;
        }



        public override bool Equals(object obj)
        {
            return Equals(obj as ToolGroup);
        }

        public bool Equals(ToolGroup other)
        {
            return other != null &&
                   slot == other.slot &&
                   page == other.page;
        }

        public override int GetHashCode()
        {
            int hashCode = -1484525425;
            hashCode = hashCode * -1521134295 + slot.GetHashCode();
            hashCode = hashCode * -1521134295 + page.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ToolGroup left, ToolGroup right)
        {
            return EqualityComparer<ToolGroup>.Default.Equals(left, right);
        }

        public static bool operator !=(ToolGroup left, ToolGroup right)
        {
            return !(left == right);
        }
    }
}
