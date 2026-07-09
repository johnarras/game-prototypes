using OxDb.MapServer.AI.Services;
using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Maps;
using OxDb.MapServer.Spells.Services;
using OxDb.MapServer.Units.Services;
using OxDb.ServerGame.Achievements;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using System.Collections.Generic;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public abstract class BaseSpellEffectHandler : ISpellEffectHandler
    {

        protected IServerSpellService _spellService = null;
        protected IMapMessageService _messageService = null;
        protected IMapObjectManager _objectManager = null;
        protected IServerUnitService _unitService = null;
        protected IAIService _aiService = null;
        protected IStatService _statService = null;
        protected IAchievementService _achievementService;
        public virtual float GetTickLength() { return SpellConstants.BaseTickSeconds; }
        public abstract List<ActiveSpellEffect> CreateEffects(MapObject obj, SpellHit spellHit);
        public abstract long HelperKey { get; }
        public abstract bool HandleEffect(MapObject obj,  ActiveSpellEffect eff);
        public abstract bool IsModifyStatEffect();
        public abstract bool UseStatScaling();
    }
}


