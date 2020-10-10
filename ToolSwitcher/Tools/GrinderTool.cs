using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    public class GrinderTool : Tool
    {
        private readonly MyDefinitionId[] ids = new MyDefinitionId[4]
        {
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AngleGrinderItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AngleGrinder4Item")
        };
        protected override MyDefinitionId[] Ids => ids;

        public override string Name => "Grinder";

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private GrinderTool() : base()
        { }

        public GrinderTool(MyKeys key, int slot, int page) : base(key, slot, page)
        { }

        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return handTool is IMyAngleGrinder;
        }
    }
}
