using System;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using BF2D.Enums;

namespace BF2D.Game
{
    [Serializable]
    public class EquipmentInfo : IUtilityEntityInfo
    {
        [JsonIgnore] public string ID => this.id;
        [JsonProperty] private readonly string id = string.Empty;
        [JsonIgnore] public int Count => this.count;
        [JsonProperty] protected int count = 0;

        [JsonIgnore] public Sprite Icon => GameCtx.Instance.GetIcon(this.GetUtility().SpriteID);

        [JsonIgnore] public string Name => Get().Name;

        [JsonIgnore] public string Description => Get().Description;

        [JsonIgnore] public IEnumerable<Enums.AuraType> Auras => Get().Auras;

        [JsonIgnore] public bool Generated => Strings.System.IsGeneratedID(this.ID);

        [JsonConstructor]
        public EquipmentInfo() { }

        public EquipmentInfo(string id)
        {
            this.id = id;
        }

        public Equipment Get() => GameCtx.Instance.GetEquipment(this.ID);

        public Entity GetEntity() => Get();

        public IUtilityEntity GetUtility() => Get();

        public int Increment()
        {
            return ++this.count;
        }

        public int Decrement()
        {
            return --this.count;
        }
    }
}
