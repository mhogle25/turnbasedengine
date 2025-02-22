using System.Collections.Generic;

namespace BF2D.Game
{
    public interface IEquipmentHolder : IUtilityEntityHolder<EquipmentInfo>
    {
        /// <summary>
        /// Removes a single equipment from the holder. This method will delete the equipment's datafile if its count reaches zero and if it is a generated equipment. Use when customizing (transforming) or deleting an equipment.
        /// </summary>
        /// <param name="info">The equipment info to remove</param>
        public void Destroy(EquipmentInfo info);

        /// <summary>
        /// Removes a single equipment from the holder. This method will delete the equipment's datafile if its count reaches zero and if it is a generated equipment. Use when customizing (transforming) or deleting an equipment.
        /// </summary>
        /// <param name="id">The equipment info id to remove</param>
        public void Destroy(string id);

        public IEnumerable<EquipmentInfo> FilterByType(Enums.EquipmentType type);
    }
}