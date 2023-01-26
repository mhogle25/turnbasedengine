using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF2D.Game.Enums;
using Newtonsoft.Json;

namespace BF2D.Game
{
    public class Equipment : PersistentEffect, ISpriteID
    {
        [JsonIgnore] public string SpriteID { get { return this.spriteID; } }
        [JsonProperty] private readonly string spriteID = string.Empty;
        [JsonIgnore] public EquipmentType Type { get { return this.equipmentType; } }
        [JsonProperty] private readonly EquipmentType equipmentType = EquipmentType.Accessory;
    }
}
