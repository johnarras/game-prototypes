using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Spawns.WorldData;

namespace OxDb.SharedGame.MapServer.Services
{
    public interface IMapProvider : IInjectable
    {
        Map GetMap();
        void SetMap(Map map);

        MapSpawnData GetSpawns();
        void SetSpawns(MapSpawnData mapSpawnData);

    }
}


