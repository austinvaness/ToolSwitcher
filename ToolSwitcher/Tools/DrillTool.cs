using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    public class DrillTool : Tool
    {
        private readonly MyDefinitionId[] ids = new MyDefinitionId[4]
        {
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/HandDrillItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/HandDrill2Item"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/HandDrill3Item"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/HandDrill4Item")
        };
        protected override MyDefinitionId[] Ids => ids;

        public override string Name => "Drill";

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        public DrillTool() : base()
        { }

        public DrillTool(MyKeys key, int slot, int page) : base(key, slot, page)
        { }

        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return handTool is IMyHandDrill;
        }
    }
}
