using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    public class RifleTool : Tool
    {
        private readonly MyDefinitionId[] ids = new MyDefinitionId[4]
        {
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AutomaticRifleItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/RapidFireAutomaticRifleItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/PreciseAutomaticRifleItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/UltimateAutomaticRifleItem")
        };
        protected override MyDefinitionId[] Ids => ids;

        public override string Name => "Rifle";

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private RifleTool() : base()
        {
            EquipIndex = 3;
        }

        public RifleTool(MyKeys key, int slot, int page) : base(key, slot, page)
        {
            EquipIndex = 3;
        }


        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return handTool is IMyAutomaticRifleGun;
        }
    }
}
