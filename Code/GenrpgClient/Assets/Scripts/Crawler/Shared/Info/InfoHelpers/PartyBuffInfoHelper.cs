using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Buffs.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class PartyBuffInfoHelper : BaseInfoHelper<PartyBuffSettings, PartyBuff>
    {
        private ICrawlerService _crawlerService = null;
        private ICrawlerSpellService _spellService = null;

        public override long HelperKey => EntityTypes.PartyBuff;

        protected override bool MakeEntityNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);

            PartyData party = _crawlerService.GetParty();


            PartyBuffSettings buffSettings = _gameData.Get<PartyBuffSettings>(_gs.ch);
            PartyBuff buff = buffSettings.Get(entityId);


            if (buff != null)
            {
                lines.Add($"\nCurrent Power: {party.Buffs[entityId].ToString("F2")}");
                lines.Add($"Power scales with the Sqrt of the party's max level.");
                lines.Add($"This is independent of role scaling to avoid forcing");
                lines.Add("Players to specialize to max their buffs.");

                CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Effects.Count == 1 &&
                x.Effects[0].EntityTypeId == EntityTypes.PartyBuff && x.Effects[0].EntityId == entityId);

                if (spell != null)
                {
                    lines.Add(_spellService.RolesThatCanCastString(spell.IdKey));
                }
            }

            return lines;
        }
    }
}


