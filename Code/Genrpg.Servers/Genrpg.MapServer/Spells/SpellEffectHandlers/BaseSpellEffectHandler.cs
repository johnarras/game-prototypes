using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spells.Services;
using Genrpg.MapServer.Units.Services;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
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
        public abstract List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit spellHit);
        public abstract long HelperKey { get; }
        public abstract bool HandleEffect(IRandom rand, ActiveSpellEffect eff);
        public abstract bool IsModifyStatEffect();
        public abstract bool UseStatScaling();
    }
}


