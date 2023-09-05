using BF2D.UI;
using BF2D.Game.Actions;
using System.Collections.Generic;
using BF2D.Game.Combat.Enums;
using UnityEngine;
using System;

namespace BF2D.Game.Combat.Actions
{
    public class CombatAction
    {
        public CombatActionType Type => this.type;
        private CombatActionType type = CombatActionType.Event;
        /// <summary>
        /// Resets all other combat action types referenced in this object
        /// </summary>
        public ActCombatActionInfo Act 
        { 
            get 
            { 
                return this.Type == CombatActionType.Act ? this.actCombatAction : null; 
            } 
            set 
            {
                this.type = CombatActionType.Act;
                this.actCombatAction = value;
            } 
        }
        private ActCombatActionInfo actCombatAction = null;
        /// <summary>
        /// Resets all other combat action types referenced in this object
        /// </summary>
        public EquipCombatActionInfo Equip 
        { 
            get 
            { 
                return this.Type == CombatActionType.Equip ? this.equipCombatAction : null; 
            } 
            set 
            {
                this.type = CombatActionType.Equip;
                this.equipCombatAction = value; 
            } 
        }
        private EquipCombatActionInfo equipCombatAction = null;
        /// <summary>
        /// Resets all other combat action types referenced in this object
        /// </summary>
        public EventCombatActionInfo Event { 
            get 
            { 
                return this.Type == CombatActionType.Event ? this.eventCombatAction : null; 
            } 
            set
            {
                this.type = CombatActionType.Event;
                this.eventCombatAction = value; 
            } 
        }
        private EventCombatActionInfo eventCombatAction = null;
        /// <summary>
        /// Resets all other combat action types referenced in this object
        /// </summary>
        public FleeCombatActionInfo Flee 
        { 
            get 
            { 
                return this.Type == CombatActionType.Flee ? this.fleeCombatAction : null; 
            } 
            set
            {
                this.type = CombatActionType.Flee;
                this.fleeCombatAction = value; 
            } 
        }
        private FleeCombatActionInfo fleeCombatAction = null;
        /// <summary>
        /// Resets all other combat action types referenced in this object
        /// </summary>
        public ItemCombatActionInfo Item 
        { 
            get 
            { 
                return this.Type == CombatActionType.Item ? this.itemCombatAction : null; 
            } 
            set
            {
                this.type = CombatActionType.Item;
                this.itemCombatAction = value; 
            } 
        }
        private ItemCombatActionInfo itemCombatAction = null;
        /// <summary>
        /// Resets all other combat action types referenced in this object
        /// </summary>
        public RosterCombatActionInfo Roster 
        { 
            get => this.Type == CombatActionType.Roster ? this.rosterCombatAction : null; 
            set 
            {

                this.type = CombatActionType.Roster;
                this.rosterCombatAction = value;
            } 
        }
        private RosterCombatActionInfo rosterCombatAction = null;

        public ICombatActionInfo CurrentInfo => this.type switch
        {
            CombatActionType.Flee => this.Flee,
            CombatActionType.Act => this.Act,
            CombatActionType.Item => this.Item,
            CombatActionType.Roster => this.Roster,
            CombatActionType.Equip => this.Equip,
            CombatActionType.Event => this.Event,
            _ => null,
        };

        public void SetupControlled()
        {
            switch (this.type)
            {
                case CombatActionType.Act: break;   //TODO
                case CombatActionType.Equip:
                    CombatCtx.One.RunCombatEvents();
                    break;
                case CombatActionType.Event: break; //TODO
                case CombatActionType.Flee: break;  //TODO
                case CombatActionType.Roster: break;
                default: throw new Exception($"[CombatAction:GetStatsAction] {this.type} should be set up with a targeter");
            }
        }

        public void SetupAI(CharacterCombatAI ai)
        {
            switch (this.type)
            {
                case CombatActionType.Act: break;   //TODO
                case CombatActionType.Equip:
                    CombatCtx.One.RunCombatEvents();
                    break;
                case CombatActionType.Event: break; //TODO
                case CombatActionType.Flee: break;  //TODO
                case CombatActionType.Item:
                    ai.SetupTargetedGems();
                    break;
                case CombatActionType.Roster: break;
            }
        }

        public IEnumerable<TargetedCharacterActionSlot> GetTargetedGems() => this.type switch
        {
            CombatActionType.Act => null,//TODO
            CombatActionType.Item => this.Item.TargetedGemSlots,
            _ => throw new Exception("[CombatAction:GetTargetedGems] Tried to get the list of gems but the CombatAction was a type other than Act or Item."),
        };
        

        public IEnumerable<TargetedCharacterActionSlot> UseTargetedGems() => this.type switch
        {
            CombatActionType.Act => null,//TODO
            CombatActionType.Item => this.Item.UseTargetedGems(),
            _ => throw new Exception("[CombatAction:GetTargetedGems] Tried to get the list of gems but the CombatAction was a type other than Act or Item."),
        };

        public IEnumerable<CharacterActionSlot> GetGems() => this.type switch
        {
            CombatActionType.Equip => this.Equip.OnEquip,
            _ => throw new Exception("[CombatAction:GetGems] Tried to get the list of gems but the CombatAction was a type other than Act or Item."),
        };

        public List<string> Run() => this.type switch
        {
            CombatActionType.Equip => this.Equip.Run(),
            _ => throw new Exception("[CombatAction:GetGems] Tried to get the list of gems but the CombatAction was a type other than Act or Item."),
        };
    }
}
