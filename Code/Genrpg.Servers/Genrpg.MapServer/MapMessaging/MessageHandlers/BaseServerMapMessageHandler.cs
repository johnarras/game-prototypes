
using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spells.Services;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseServerMapMessageHandler<TMapObject, TMapMessage> : IMapMessageHandler
        where TMapMessage : class, IMapMessage
        where TMapObject : MapObject
    {
        public Type HelperKey => typeof(TMapMessage);

        protected IMapMessageService _messageService = null;
        protected IMapObjectManager _objectManager = null;
        protected IServerSpellService _spellService = null;
        protected IAIService _aiService = null;
        protected IRewardService _rewardService = null;
        protected ICloudCommsService _cloudCommsService = null;
        protected ILogService _logService = null;
        protected IFullRepositoryService _repoService = null;
        protected IGameData _gameData;

        public virtual void Setup(IServiceLocator loc)
        {
        }

        protected abstract Task InnerProcess(IRandom rand, MapMessagePackage pack, TMapObject obj, TMapMessage message);

        public async Task Process(IRandom rand, MapMessagePackage pack)
        {
            if (!pack.message.IsCancelled() && pack.mapObject is TMapObject tMapObject)
            {
                await InnerProcess(rand, pack, tMapObject, pack.message as TMapMessage);
            }
        }
    }
}


