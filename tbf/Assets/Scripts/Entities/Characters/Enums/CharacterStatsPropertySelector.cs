using System.Runtime.Serialization;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BF2D.Game.Enums
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CharacterStatsProperty
    {
        [EnumMember]
        Speed,
        [EnumMember]
        Attack,
        [EnumMember]
        Defense,
        [EnumMember]
        Focus,
        [EnumMember]
        Luck,
        [EnumMember]
        MaxHealth,
        [EnumMember]
        MaxStamina
    }

    public class CharacterStatsPropertyCollection<T>
    {
        private T[] collection;
        private readonly int enumSize = 0;

        public CharacterStatsPropertyCollection()
        {
            enumSize = Enum.GetValues(typeof(CharacterStatsProperty)).Length;
            collection = new T[enumSize];
        }

        public T this[CharacterStatsProperty index]
        {
            get
            {
                return this.collection[(int)index];
            }

            set
            {
                this.collection[(int)index] = value;
            }
        }
    }

    public class CharacterStatsPropertySelector : MonoBehaviour
    {
        public CharacterStatsProperty characterStatsProperty;
    }
}