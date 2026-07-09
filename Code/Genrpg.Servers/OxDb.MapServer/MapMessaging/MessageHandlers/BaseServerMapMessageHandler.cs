using OxDb.MapServer.AI.Services;
using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Maps;
using OxDb.MapServer.Spells.Services;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Rewards.Services;
using System;
using System.Threading.Tasks;

namespace OxDb.MapServer.MapMessaging.MessageHandlers
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

        protected abstract ValueTask InnerProcess(TMapObject obj, TMapMessage message);

        public async ValueTask Process(MapMessagePackage pack)
        {
            if (!pack.Message.IsCancelled() && pack.MapObject is TMapObject tMapObject && pack.Message is TMapMessage tMapMessage)
            {
                await InnerProcess(tMapObject, tMapMessage);
            }
        }
    }
}


