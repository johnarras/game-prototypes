using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;

namespace OxDb.SharedGame.Characters.PlayerData
{
    public interface ICoreCharacter : IFilteredObject, IVersionedData
    {
        string Name { get; set; }
        string UserId { get; set; }
        string MapId { get; set; }
        int Version { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float Rot { get; set; }
        float Speed { get; set; }
        long ZoneId { get; set; }
        long FactionTypeId { get; set; }
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        long SexTypeId { get; set; }

    }

}


