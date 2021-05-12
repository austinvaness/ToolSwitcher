using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    public class LauncherTool : Tool
    {
        private readonly MyDefinitionId[] ids = new MyDefinitionId[2]
        {
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/BasicHandHeldLauncherItem"),
            MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/AdvancedHandHeldLauncherItem"),
        };
        protected override MyDefinitionId[] Ids => ids;

        public override string Name => "Rocket Launcher";

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        private LauncherTool() : base()
        {
            EquipIndex = 1;
        }

        public LauncherTool(MyKeys key, int slot, int page) : base(key, slot, page)
        {
            EquipIndex = 1;
        }


        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return handTool is IMyAutomaticRifleGun;
        }
    }
}
