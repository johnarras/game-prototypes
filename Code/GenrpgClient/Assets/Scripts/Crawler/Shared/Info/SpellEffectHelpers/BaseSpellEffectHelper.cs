using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Crawler.Info.EffectHelpers;
using OxDb.SharedGame.Crawler.Info.Services;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Services;

namespace OxDb.SharedGame.Crawler.Info.SpellEffectHelpers
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


