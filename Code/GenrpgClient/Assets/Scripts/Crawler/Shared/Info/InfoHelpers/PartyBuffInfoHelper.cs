using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
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

        public override long Key => EntityTypes.PartyBuff;

        protected override bool MakeNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);

            PartyData party = _crawlerService.GetParty();

            PartyBuff buff = _gameData.Get<PartyBuffSettings>(_gs.ch).Get(entityId);


            if (buff != null)
            {
                CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Effects.Count == 1 &&
                x.Effects[0].EntityTypeId == EntityTypes.PartyBuff && x.Effects[0].EntityId == entityId);

                if (spell != null)
                {
                    RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

                    if (scalingType != null)
                    {
                        lines.Add($"\nCurrent Power: {party.Buffs.Get(entityId).ToString("F2")}");
                        lines.Add($"\nScales with Sqrt({scalingType.Name}) Role Scaling.");
                        lines.Add("The 'Party Buff' button casts the highest ");
                        lines.Add("power possible, but will not overwrite ");
                        lines.Add("an existing better buff.");
                    }
                }
            }

            return lines;
        }

    }
}
