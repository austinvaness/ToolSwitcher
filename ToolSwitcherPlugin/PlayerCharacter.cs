﻿using avaness.ToolSwitcherPlugin.Definitions;
using avaness.ToolSwitcherPlugin.Slot;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.Entity;

namespace avaness.ToolSwitcherPlugin
{
    public class PlayerCharacter
    {
        public PlayerToolbar Toolbar { get; private set; }
        public MyInventory Inventory { get; private set; }

        private readonly MyPlayer p;
        private readonly ToolDefinitions defs;
        private MyCharacter ch;

        //public event Action<HandItem, ToolSlot> ItemRemoved;
        //public event Action<HandItem, IEnumerable<ToolSlot>> ItemAdded;

        public PlayerCharacter(MyPlayer p, ToolDefinitions defs)
        {
            this.p = p;
            this.defs = defs;
            Subscribe(p.Character);
            p.Controller.ControlledEntityChanged += Controller_ControlledEntityChanged;
        }

        public void Unload()
        {
            p.Controller.ControlledEntityChanged -= Controller_ControlledEntityChanged;
            //ItemRemoved = null;
            //ItemAdded = null;
            Toolbar?.Unload();
        }

        private void Subscribe(MyCharacter ch)
        {
            if(ch != null && (this.ch == null || ch != this.ch))
            {
                if (this.ch != null)
                    Ch_OnClose(this.ch);
                ch.OnClosing += Ch_OnClose;
                Inventory = ch.GetInventory();
                //Inventory.ContentsAdded += Inv_ContentsChanged;
                //ch.Toolbar.ItemUpdated += Toolbar_ItemUpdated;
                Toolbar = new PlayerToolbar(ch.Toolbar, defs);
                this.ch = ch;
            }
        }

        /*private void Toolbar_ItemUpdated(MyToolbar toolbar, MyToolbar.IndexArgs slot, MyToolbarItem.ChangeInfo change)
        {
            HandItem item;
            if(ItemRemoved != null && change == MyToolbarItem.ChangeInfo.Enabled && TryGetHandItem(toolbar, slot.ItemIndex, out item))
                ItemRemoved.Invoke(item, new ToolSlot(slot.ItemIndex, defs));
        }*/

        private bool TryGetHandItem(MyToolbar toolbar, int slotIndex, out HandItem hand)
        {
            MyToolbarItemDefinition item = toolbar.GetItemAtIndex(slotIndex) as MyToolbarItemDefinition;
            if (item != null)
            {
                MyHandItemDefinition handItemDef = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(item.Definition.Id);
                if (handItemDef != null)
                {
                    hand = new HandItem(handItemDef.Id, item.Definition.Id);
                    return true;
                }
            }
            hand = null;
            return false;
        }

        private bool TryGetHandItem(MyPhysicalInventoryItem item, out HandItem hand)
        {
            hand = null;
            MyObjectBuilder_PhysicalGunObject physicalItem = item.Content as MyObjectBuilder_PhysicalGunObject;
            if (physicalItem == null)
                return false;
            MyDefinitionId physicalItemId = new MyDefinitionId(physicalItem.TypeId, physicalItem.SubtypeId);
            MyHandItemDefinition handItem = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(physicalItemId);
            if (handItem == null)
                return false;
            hand = new HandItem(handItem.Id, physicalItemId);
            return true;
        }

        /*private void Inv_ContentsChanged(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            HandItem hand;
            if(ItemAdded != null && TryGetHandItem(item, out hand))
            {
                List<ToolSlot> slots = new List<ToolSlot>();
                MyToolbar toolbar = ch.Toolbar;
                for(int i = 0; i < toolbar.ItemCount; i++)
                {
                    MyToolbarItemDefinition slotDef = toolbar.GetItemAtIndex(i) as MyToolbarItemDefinition;
                    if(slotDef != null)
                    {

                    }
                }
                if(slots.Count > 0)
                    ItemAdded.Invoke(hand, slots);
            }
        }*/

        private void Ch_OnClose(MyEntity e)
        {
            ch.OnClosing -= Ch_OnClose;
            //Inventory.ContentsAdded -= Inv_ContentsChanged;
            //ch.Toolbar.ItemUpdated -= Toolbar_ItemUpdated;
            Toolbar.Unload();
            Toolbar = null;
            ch = null;
            Inventory = null;
        }

        private void Controller_ControlledEntityChanged(IMyControllableEntity from, IMyControllableEntity to)
        {
            if(to != null)
                Subscribe(to.Entity as MyCharacter);
        }
    }
}