using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BF2D.Game
{
    [Serializable]
    public class Party
    {
        [JsonIgnore] public CharacterStats PartyLeader
        {
            get
            {
                CharacterStats smallestGridPosition = this.activeCharacters[0];
                foreach (CharacterStats character in this.activeCharacters)
                    if (character.GridPosition < smallestGridPosition.GridPosition)
                        smallestGridPosition = character;
                return smallestGridPosition;
            }
        }
        [JsonIgnore] public IEnumerable<CharacterStats> ActiveCharacters => this.activeCharacters;
        [JsonProperty] private readonly List<CharacterStats> activeCharacters = new();

        [JsonIgnore] public IEnumerable<CharacterStats> InactiveCharacters => this.inactiveCharacters;
        [JsonProperty] private readonly List<CharacterStats> inactiveCharacters = new();

        [JsonIgnore] public IItemHolder Items => this.items;
        [JsonProperty] private readonly ItemHolder items = new();

        [JsonIgnore] public IEquipmentHolder Equipments => this.equipments;
        [JsonProperty] private readonly EquipmentHolder equipments = new();

        [JsonIgnore] public int Currency { get => this.currency; set => this.currency = value; }
        [JsonProperty] private int currency = 0;

        [JsonIgnore] public int Ether { get => this.ether; set => this.ether = value; }
        [JsonProperty] private int ether = 0;

        public CharacterStats GetCharacter(string id)
        {
            return this.activeCharacters.Find(character => character.ID == id);
        }

        public void AddCharacter(CharacterStats newCharacter)
        {
            if (newCharacter is null)
            {
                Debug.LogError("[SaveData:AddPlayer] Tried to add a player but the player was null");
                return;
            }

            if (this.activeCharacters.Count >= Numbers.MaxPartySize)
            {
                Debug.LogError("[SaveData:AddPlayer] Tried to add a player but the maximum number of active players was reached.");
                return;
            }

            this.activeCharacters.Add(newCharacter);
        }
    }
}