using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spawns.Interfaces;

namespace OxDb.SharedGame.MapObjects.Factories
{
    public interface IMapObjectFactory : ISetupDictionaryItem<long>
    {
        MapObject Create(IRandom rand, IMapSpawn spawn);

    }
}


