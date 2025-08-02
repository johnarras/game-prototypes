
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.AI.Services;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.Spells.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Services;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseServerMapMessageHandler<TMapObject,TMapMessage> : IMapMessageHandler 
        where TMapMessage : class, IMapMessage
        where TMapObject : MapObject
    { 
        public Type Key => typeof(TMapMessage);

        protected IMapMessageService _messageService = null;
        protected IMapObjectManager _objectManager = null;
        protected IServerSpellService _spellService = null;
        protected IAIService _aiService = null;
        protected IRewardService _rewardService = null;
        protected ICloudCommsService _cloudCommsService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IGameData _gameData;

        public virtual void Setup(IServiceLocator loc)
        {
        }

        protected abstract void InnerProcess(IRandom rand, MapMessagePackage pack, TMapObject obj, TMapMessage message);

        public void Process(IRandom rand, MapMessagePackage pack)
        {
            if (!pack.message.IsCancelled() && pack.mapObject is TMapObject tMapObject)
            {
                InnerProcess(rand, pack, tMapObject, pack.message as TMapMessage);
            }
        }
    }
}
