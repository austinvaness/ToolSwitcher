using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Screens.Helpers;
using VRage.Game;

namespace avaness.ToolSwitcherPlugin.Tools
{
    public interface ITool
    {
        MyEngineerToolBaseDefinition[] Defs { get; }

        void Equip(HandItem hand, MyInventory inv, MyToolbar toolbar, ToolSlot slot);
        bool Contains(HandItem hand);
        bool Contains(MyDefinitionId id);
    }
}