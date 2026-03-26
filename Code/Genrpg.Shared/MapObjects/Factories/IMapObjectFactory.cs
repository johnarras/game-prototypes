using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.MapObjects.Factories
{
    public interface IMapObjectFactory : ISetupDictionaryItem<long>
    {
        MapObject Create(IRandom rand, IMapSpawn spawn);

    }
}


