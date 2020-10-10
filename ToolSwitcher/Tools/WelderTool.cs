using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    public class WelderTool : Tool
    {
        private readonly MyDefinitionId[] ids = new MyDefinitionId[4]
        {
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/WelderItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/Welder2Item"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/Welder3Item"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/Welder4Item")
        };
        protected override MyDefinitionId[] Ids => ids;

        public override string Name => "Welder";

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private WelderTool() : base()
        { }

        public WelderTool(MyKeys key, int slot, int page) : base(key, slot, page)
        { }

        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return handTool is IMyWelder;
        }
    }
}
