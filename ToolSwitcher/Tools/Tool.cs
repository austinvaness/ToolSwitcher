using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Input;

namespace avaness.ToolSwitcher.Tools
{
    [XmlInclude(typeof(GrinderTool))]
    [XmlInclude(typeof(WelderTool))]
    [XmlInclude(typeof(DrillTool))]
    [XmlInclude(typeof(RifleTool))]
    [XmlInclude(typeof(ModTool))]
    public abstract class Tool : IEquatable<Tool>, IComparable<Tool>
    {
        protected abstract MyDefinitionId[] Ids { get; }
        protected IMyPlayer p = MyAPIGateway.Session.Player;

        [XmlIgnore]
        public abstract string Name { get; }

        [XmlIgnore]
        public ToolMenu Menu;

        [XmlElement]
        public MyKeys Keybind { get; set; }

        [XmlElement]
        public int Slot { get; set; }

        [XmlElement]
        public int Page { get; set; }

        [XmlElement]
        public bool Enabled { get; set; }

        [XmlIgnore]
        public int EquipIndex { get; set; } = 0; // For creative mode.

        public virtual bool CanScroll { get; } = true;


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
                Equip();
            return false;
        }

        public bool Upgrade()
        {
            if (!Enabled)
                return false;

            MyDefinitionId handId;
            if (GetHandId(out handId))
            {
                // Something is in the hand
                int index;
                if (Ids.Length > 1 && GetIndex(handId, out index))
                {
                    // The item in the hand is correct type
                    if (Upgrade(ref handId, index))
                    {
                        // Item has a better version
                        SetToolSlot(handId);
                        EquipSlot();
                        return true;
                    }
                    else
                    {
                        // Item does not have a better version
                        return false;
                    }
                }
                else
                {
                    // The item in the hand is not correct type
                    return false;
                }

            }
            else
            {
                // Nothing in the hand
                return false;
            }
        }

        public bool Downgrade()
        {
            if (!Enabled)
                return false;

            MyDefinitionId handId;
            if (GetHandId(out handId))
            {
                // Something is in the hand
                int index;
                if (Ids.Length > 1 && GetIndex(handId, out index))
                {
                    // The item in the hand is correct type
                    if (Downgrade(ref handId, index))
                    {
                        // Item has an lesser version
                        SetToolSlot(handId);
                        EquipSlot();
                        return true;
                    }
                    else
                    {
                        // Item does not have a lesser version
                        return false;
                    }
                }
                else
                {
                    // The item in the hand is not correct type
                    return false;
                }

            }
            else
            {
                // Nothing in the hand
                return false;
            }
        }

        public bool Equip(MyDefinitionId? itemRemoved = null)
        {
            if (!Enabled)
                return false;

            MyDefinitionId handId;
            if (!itemRemoved.HasValue && GetHandId(out handId))
            {
                // Something is in the hand
                int index;
                if (GetIndex(handId, out index))
                {
                    // The item in the hand is correct type
                    if (MakeBest(ref handId, index))
                    {
                        // The item needs to be upgraded
                        SetToolSlot(handId);
                        EquipSlot();
                        return true;
                    }
                    else
                    {
                        // The item in the hand is already the best
                        SelectPage();
                        return true;
                    }
                }
                else
                {
                    // The item in the hand is not correct type
                    return EquipBest();
                }

            }
            else
            {
                // Nothing in the hand
                return EquipBest(itemRemoved);
            }
        }

        private bool EquipBest(MyDefinitionId? itemRemoved = null)
        {
            MyDefinitionId handId;
            if (GetBestId(out handId, itemRemoved))
            {
                // An item exists in the inventory
                SetToolSlot(handId);
                EquipSlot();
                return true;
            }
            else
            {
                // No items exist in the inventory
                return false;
            }
        }

        private bool GetHandId(out MyDefinitionId handId)
        {
            IMyHandheldGunObject<MyDeviceBase> tool = GetHandTool();
            if (tool == null)
            {
                handId = new MyDefinitionId();
                return false;
            }

            handId = tool.PhysicalItemDefinition.Id;
            return true;
        }

        private bool GetIndex(MyDefinitionId id, out int index)
        {
            for (int i = 0; i < Ids.Length; i++)
            {
                if (Ids[i] == id)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        /// <summary>
        /// Finds the best available id.
        /// Returns true if an id was found.
        /// </summary>
        /// <param name="itemRemoved">The item to ignore when checking inventory.</param>
        private bool GetBestId(out MyDefinitionId id, MyDefinitionId? itemRemoved = null)
        {
            if(MyAPIGateway.Session.CreativeMode)
            {
                id = Ids[EquipIndex];
                return true;
            }

            IMyInventory inv = p.Character.GetInventory();
            MyFixedPoint one = 1;

            for (int i = Ids.Length - 1; i >= 0; i--)
            {
                MyDefinitionId temp = Ids[i];
                if ((!itemRemoved.HasValue || itemRemoved.Value != temp) && inv.ContainItems(one, temp))
                {
                    id = temp;
                    return true;
                }
            }

            id = new MyDefinitionId();
            return false;
        }

        /// <summary>
        /// Finds the next lower level tool.
        /// Returns true if the id was changed.
        /// </summary>
        private bool Downgrade(ref MyDefinitionId id, int index)
        {
            if (MyAPIGateway.Session.CreativeMode)
            {
                if (index == 0)
                    return false;

                EquipIndex = index - 1;
                id = Ids[EquipIndex];
                return true;
            }

            IMyInventory inv = p.Character.GetInventory();
            MyFixedPoint one = 1;

            for (int i = index - 1; i >= 0; i--)
            {
                if (inv.ContainItems(one, Ids[i]))
                {
                    id = Ids[i];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the next higher level tool.
        /// Returns true if the id was changed.
        /// </summary>
        private bool Upgrade(ref MyDefinitionId id, int index)
        {
            if (MyAPIGateway.Session.CreativeMode)
            {
                if (index == Ids.Length - 1)
                    return false;

                EquipIndex = index + 1;
                id = Ids[EquipIndex];
                return true;
            }

            IMyInventory inv = p.Character.GetInventory();
            MyFixedPoint one = 1;

            for (int i = index + 1; i < Ids.Length; i++)
            {
                if (inv.ContainItems(one, Ids[i]))
                {
                    id = Ids[i];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the best available tool.
        /// Returns true if the id was changed.
        /// </summary>
        private bool MakeBest(ref MyDefinitionId id, int index)
        {
            if (MyAPIGateway.Session.CreativeMode)
            {
                if (index == EquipIndex)
                    return false;

                id = Ids[EquipIndex];
                return true;
            }

            IMyInventory inv = p.Character.GetInventory();
            MyFixedPoint one = 1;

            for (int i = Ids.Length - 1; i > index; i--)
            {
                if (inv.ContainItems(one, Ids[i]))
                {
                    id = Ids[i];
                    return true;
                }
            }
            return false;
        }

        private void SetToolSlot(MyDefinitionId id)
        {
            SelectPage();
            MyVisualScriptLogicProvider.SetToolbarSlotToItem(Slot, id, p.IdentityId);
        }

        private void SelectPage()
        {
            MyVisualScriptLogicProvider.SetToolbarPage(Page, p.IdentityId);
        }

        private void EquipSlot()
        {
            MyVisualScriptLogicProvider.SwitchToolbarToSlot(Slot, p.IdentityId);
        }

        public bool HasId(MyDefinitionId toolId, out int index)
        {
            for(int i = 0; i < Ids.Length; i++)
            {
                MyDefinitionId id = Ids[i];
                if (id == toolId)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public void ClearSlot()
        {
            SelectPage();
            MyVisualScriptLogicProvider.ClearToolbarSlot(Slot, p.IdentityId);
        }

        public bool IsInHand()
        {
            IMyHandheldGunObject<MyDeviceBase> handTool = GetHandTool();
            if (handTool == null || !IsHandType(handTool))
                return false;
            foreach(MyDefinitionId id in Ids)
            {
                if (id == handTool.PhysicalItemDefinition.Id)
                    return true;
            }
            return false;
        }

        protected abstract bool IsHandType(IMyHandheldGunObject<MyDeviceBase> handTool);

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

        public int CompareTo(Tool other)
        {
            return Name.CompareTo(other);
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
