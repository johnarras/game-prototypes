using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Units.Services;

namespace OxDb.SharedGame.MapObjects.Factories
{
    public abstract class BaseMapObjectFactory : IMapObjectFactory
    {
        protected IUnitGenService _unitGenService;
        protected IGameData _gameData;
        protected IRepositoryService _repoService;
        protected IMapProvider _mapProvider;
        protected ITextSerializer _serializer;

        public abstract MapObject Create(IRandom rand, IMapSpawn spawn);
        public abstract long HelperKey { get; }

    }
}


