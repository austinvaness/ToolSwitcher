using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game;

namespace avaness.ToolSwitcherPlugin.Tools
{
    public class Tool<T> : ITool where T : MyObjectBuilder_EngineerToolBaseDefinition
    {
        private readonly MyEngineerToolBaseDefinition[] defs;
        private readonly Dictionary<MyDefinitionId, int> ids;

        public MyEngineerToolBaseDefinition[] Defs => defs;

        public Tool()
        {
            SortedDictionary<string, MyEngineerToolBaseDefinition> defs = new SortedDictionary<string, MyEngineerToolBaseDefinition>();
            foreach(var def in MyDefinitionManager.Static.GetHandItemDefinitions())
            {
                if (def.GetObjectBuilder() is T)
                {
                    defs[def.Id.SubtypeName] = (MyEngineerToolBaseDefinition)def;
                }
            }
            this.defs = defs.Values.ToArray();

            this.defs = new MyEngineerToolBaseDefinition[defs.Count];
            ids = new Dictionary<MyDefinitionId, int>();
            int i = 0;
            foreach(MyEngineerToolBaseDefinition def in defs.Values)
            {
                ids[def.Id] = i;
                this.defs[i] = def; 
                i++;
            }
        }


        public bool Contains(HandItem hand)
        {
            return ids.ContainsKey(hand.Id);
        }

        public bool Contains(MyDefinitionId id)
        {
            return ids.ContainsKey(id);
        }

        public void Equip(HandItem hand, MyInventory inv, MyToolbar toolbar, ToolSlot slot)
        {
            if(hand != null)
            {
                if (hand != null && ids.TryGetValue(hand.Id, out int i))
                    EquipBest(inv, toolbar, slot, i);
                else
                    EquipBest(inv, toolbar, slot);
            }
            else
            {
                EquipBest(inv, toolbar, slot);
            }
        }

        private void EquipBest(MyInventory inv, MyToolbar toolbar, ToolSlot slot, int min = -1)
        {
            MyFixedPoint one = 1;
            for (int i = defs.Length - 1; i > min; i--)
            {
                if (MyAPIGateway.Session.CreativeMode || inv.ContainItems(one, defs[i].PhysicalItemId))
                {
                    slot.SetTo(toolbar, defs[i].PhysicalItemId);
                    return;
                }
            }
        }

        private IMyHandheldGunObject<MyDeviceBase> GetHand()
        {
            return (IMyHandheldGunObject<MyDeviceBase>)MySession.Static.LocalCharacter.EquippedTool;
        }
    }
}
