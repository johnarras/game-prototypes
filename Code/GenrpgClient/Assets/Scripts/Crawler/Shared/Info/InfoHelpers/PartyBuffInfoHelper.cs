using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class PartyBuffInfoHelper : BaseInfoHelper<PartyBuffSettings, PartyBuff>
    {
        private ICrawlerService _crawlerService = null;
        private ICrawlerSpellService _spellService = null;

        public override long Key => EntityTypes.PartyBuff;

        protected override bool MakeEntityNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);

            PartyData party = _crawlerService.GetParty();


            PartyBuffSettings buffSettings = _gameData.Get<PartyBuffSettings>(_gs.ch);
            PartyBuff buff = buffSettings.Get(entityId);


            if (buff != null)
            {
                lines.Add($"\nCurrent Power: {party.Buffs.Get(entityId).ToString("F2")}");
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
