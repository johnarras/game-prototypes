using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;

namespace OxDb.SharedGame.MapObjects.Interfaces
{
    public interface IMapObject : IFilteredObject, ISearchableItem
    {
        string Name { get; set; }
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float Rot { get; set; }
        float Speed { get; set; }
        long ZoneId { get; set; }
        string LocationId { get; set; }
        string LocationPlaceId { get; set; }
        long AddonBits { get; set; }
    }
}


