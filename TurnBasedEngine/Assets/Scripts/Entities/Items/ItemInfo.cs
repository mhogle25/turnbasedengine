using System;
using Newtonsoft.Json;
using UnityEngine;

namespace BF2D.Game
{
    [Serializable]
    public class ItemInfo : IUtilityEntityInfo
    {
        [JsonIgnore] public string ID { get { return this.id; } }
        [JsonProperty] private readonly string id = string.Empty;
        [JsonIgnore] public int Count { get { return this.count; } }
        [JsonProperty] private int count = 0;

        [JsonIgnore] private Item staged = null;

        [JsonIgnore] public Entity GetEntity { get { return Get(); } }

        [JsonIgnore] public IUtilityEntity GetUtility { get { return Get(); } }

        [JsonIgnore] public Sprite Icon { get { return GameInfo.Instance.GetIcon(GetUtility.SpriteID); } }

        [JsonIgnore] public string Name { get { return Get().Name; } }

        [JsonIgnore] public bool Useable { get { return Get().Useable; } }

        [JsonIgnore] public bool CombatExclusive { get { return Get().CombatExclusive; } }

        public ItemInfo(string id)
        {
            this.id = id;
        }

        public Item Get()
        {
            this.staged ??= GameInfo.Instance.InstantiateItem(this.id);
            return this.staged;
        }

        public void Increment()
        {
            this.count++;
        }

        public Item Use(IItemHolder owner)
        {
            Get();

            if (this.staged is null)
                return null;

            if (this.staged.Consumable)
            {
                this.count--;
                if (this.count < 1)
                    owner.RemoveItem(this);
            }

            return ResetStaged();
        }

        public Item ResetStaged()
        {
            Item item = this.staged;
            this.staged = null;
            return item;
        }
    }
}
