using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using System.Xml.Serialization;
using VRage.Game;
using VRage.Input;
using VRage.ObjectBuilders;

namespace avaness.ToolSwitcher.Tools
{
    public class ModTool : Tool
    {
        private string name;

        private MyDefinitionId[] ids;
        private MyDefinitionId id;
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

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        public ModTool() : base()
        { 
            
        }

        public ModTool(MyKeys key, int slot, int page, MyDefinitionId id) : base(key, slot, page)
        {
            Id = id;
        }

        protected override MyDefinitionId[] Ids => ids;

        protected override bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool)
        {
            return true;
        }
    }
}
