
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting
{
    public class SpecialSpellCastingStateHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.SpecialSpellCast;
        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            if (selectSpellAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing Special Select Spell" };
            }

            CrawlerSpell spell = selectSpellAction.Spell;

            CrawlerSpellEffect specialEffect = spell.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.SpecialMagic);

            if (specialEffect == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing Special Select Spell Effect" };
            }

            ISpecialMagicHelper helper = _crawlerSpellService.GetSpecialEffectHelper(specialEffect.EntityId);
            if (helper != null)
            {
                return await helper.HandleEffect(stateData, selectSpellAction, spell, specialEffect, token);
            }
            return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That spell is missing a special effect." };
        }
    }
}


