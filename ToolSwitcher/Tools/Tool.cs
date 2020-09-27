using Draygo.API;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.ModAPI;

namespace avaness.ToolSwitcher.Tools
{
    [XmlInclude(typeof(GrinderTool))]
    [XmlInclude(typeof(WelderTool))]
    [XmlInclude(typeof(DrillTool))]
    [XmlInclude(typeof(RifleTool))]
    public abstract class Tool : IEquatable<Tool>
    {
        public abstract string Name { get; }
        protected abstract MyDefinitionId[] Ids { get; }

        [XmlElement]
        public MyKeys Keybind { get; set; }

        [XmlElement]
        public int Slot { get; set; }

        [XmlElement]
        public int Page { get; set; }

        [XmlElement]
        public bool Enabled { get; set; }

        protected IMyPlayer p = MyAPIGateway.Session.Player;

        /// <summary>
        /// Used for serialization only.
        /// </summary>
        public Tool()
        {

        }

        public Tool(MyKeys key, int slot, int page)
        {
            Keybind = key;
            Slot = slot;
            Page = page;
            Enabled = true;
        }

        public bool HandleInput()
        {
            if (Enabled && MyAPIGateway.Input.IsNewKeyPressed(Keybind))
                return Equip(true);
            return false;
        }

        public bool Upgrade(MyDefinitionId currentId)
        {
            if (!Enabled)
                return false;

            MyDefinitionId newTool;
            if (TryGetNextId(currentId, out newTool))
            {
                MyVisualScriptLogicProvider.SetToolbarPage(Page, p.IdentityId);
                MyVisualScriptLogicProvider.SetToolbarSlotToItem(Slot, newTool, p.IdentityId);
                MyVisualScriptLogicProvider.SwitchToolbarToSlot(Slot, p.IdentityId);
                return true;
            }
            return false;
        }

        public bool Equip(bool switchSlot = false)
        {
            if (!Enabled)
                return false;

            MyDefinitionId newTool;
            if (TryGetNextId(out newTool))
            {
                MyVisualScriptLogicProvider.SetToolbarPage(Page, p.IdentityId);
                MyVisualScriptLogicProvider.SetToolbarSlotToItem(Slot, newTool, p.IdentityId);
                MyVisualScriptLogicProvider.SwitchToolbarToSlot(Slot, p.IdentityId);
                return true;
            }

            if(switchSlot)
            {
                MyVisualScriptLogicProvider.SetToolbarPage(Page, p.IdentityId);
                MyVisualScriptLogicProvider.SwitchToolbarToSlot(Slot, p.IdentityId);
            }
            return false;
        }

        public bool HasId(MyDefinitionId toolId)
        {
            foreach(MyDefinitionId id in Ids)
            {
                if (id == toolId)
                    return true;
            }
            return false;
        }

        public void ClearSlot()
        {
            MyVisualScriptLogicProvider.SetToolbarPage(Page, p.IdentityId);
            MyVisualScriptLogicProvider.ClearToolbarSlot(Slot, p.IdentityId);
        }

        private bool TryGetNextId(out MyDefinitionId newTool)
        {
            MyDefinitionId existing;
            TryGetCurrentId(out existing);
            return TryGetNextId(existing, out newTool);
        }

        private bool TryGetNextId(MyDefinitionId existing, out MyDefinitionId newTool)
        {
            int currentId = -1;
            for (int i = 0; i < Ids.Length; i++)
            {
                if (Ids[i] == existing)
                {
                    currentId = i;
                    break;
                }
            }

            if (MyAPIGateway.Session.CreativeMode)
            {
                newTool = Ids.Last();
                return currentId < 0 || newTool != existing;
            }

            IMyInventory inv = p.Character.GetInventory();

            for (int i = Ids.Length - 1; i > currentId; i--)
            {
                if (inv.ContainItems(1, Ids[i]))
                {
                    newTool = Ids[i];
                    return true;
                }
            }

            newTool = new MyDefinitionId();
            return false;
        }

        protected bool TryGetCurrentId(out MyDefinitionId existing)
        {
            IMyHandheldGunObject<MyDeviceBase> tool = GetHandTool();
            if (tool == null)
            {
                existing = new MyDefinitionId();
                return false;
            }
            existing = tool.PhysicalItemDefinition.Id;
            return true;
        }

        public abstract bool IsInHand();

        private IMyHandheldGunObject<MyDeviceBase> GetHandTool()
        {
            return p.Character.EquippedTool as IMyHandheldGunObject<MyDeviceBase>;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Tool);
        }

        public bool Equals(Tool other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(Tool left, Tool right)
        {
            return EqualityComparer<Tool>.Default.Equals(left, right);
        }

        public static bool operator !=(Tool left, Tool right)
        {
            return !(left == right);
        }
    }
}
