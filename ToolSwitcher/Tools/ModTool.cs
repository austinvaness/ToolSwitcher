using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System.Xml.Serialization;
using VRage.Game;
using VRage.Input;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace avaness.ToolSwitcher.Tools
{
    public class ModTool : Tool
    {
        private static MyDefinitionId PaintGun = MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/PhysicalPaintGun");
        private static MyDefinitionId ConcreteTool = MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/PhysicalConcreteTool");

        private bool allowShift;
        private string name;

        private MyDefinitionId[] ids;
        private MyDefinitionId id;

        [XmlIgnore]
        public string ModName { get; set; }

        [XmlElement]
        public SerializableDefinitionId Id
        {
            get
            {
                return id;
            }
            set
            {
                ids = new MyDefinitionId[1] { value };
                id = value;
                name = id.SubtypeName;
                if (name.StartsWith("Physical") && name.Length > 8)
                    name = name.Substring(8);
                if (name.EndsWith("Item") && name.Length > 4)
                    name = name.Substring(0, name.Length - 4);
                else if (name.EndsWith("Gun") && name.Length > 3)
                    name = name.Substring(0, name.Length - 3);
                name = name.Replace('_', ' ');
            }
        }

        public override string Name => name;

        public override bool CanScroll  => allowShift || !MyAPIGateway.Input.IsAnyShiftKeyPressed();

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        public ModTool() : base()
        { 
            
        }

        public ModTool(MyKeys key, int slot, int page, MyDefinitionId id, string modName) : base(key, slot, page)
        {
            Id = id;
            allowShift = id != PaintGun && id != ConcreteTool;
            this.ModName = modName;
        }

        protected override MyDefinitionId[] Ids => ids;

        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return true;
        }
    }
}
