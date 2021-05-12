using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    public class PistolTool : Tool
    {
        private readonly MyDefinitionId[] ids = new MyDefinitionId[3]
        {
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/SemiAutoPistolItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/FullAutoPistolItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/ElitePistolItem")
        };
        protected override MyDefinitionId[] Ids => ids;

        public override string Name => "Pistol";

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private PistolTool() : base()
        {
            EquipIndex = 2;
        }

        public PistolTool(MyKeys key, int slot, int page) : base(key, slot, page)
        {
            EquipIndex = 2;
        }


        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return handTool is IMyAutomaticRifleGun;
        }
    }
}
