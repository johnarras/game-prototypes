using Genrpg.Shared.Crawler.Info.EffectHelpers;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public abstract class BaseSpellEffectHelper : ISpellEffectHelper
    {
        protected IInfoService _infoService = null;
        protected IEntityService _entityService = null;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected ICrawlerService _crawlerService = null;


        public abstract long HelperKey { get; }
        public abstract string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect);

        protected virtual string GetRoleScalingText(CrawlerSpell spell, CrawlerSpellEffect effect, string prefix = " per ")
        {
            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

            if (scalingType != null)
            {
                return prefix + $"{_infoService.CreateInfoLink(scalingType)} Tier";
            }
            else
            {
                return "";
            }

        }
    }
}


