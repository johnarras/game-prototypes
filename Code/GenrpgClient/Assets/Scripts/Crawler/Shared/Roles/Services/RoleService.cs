using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Roles.Services
{

    public interface IRoleService : IInjectable
    {

        double GetRoleScalingLevel(PartyData party, CrawlerUnit crawlerUnit, long roleScalingTypeId);
        double GetSpellScalingLevel (PartyData party, CrawlerUnit crawlerUnit, CrawlerSpell spell);

    }

    public class RoleService : IRoleService
    {

        private IGameData _gameData;
        private IClientGameState _gs;
        private ILogService _logService;

        private ICrawlerUpgradeService _upgradeService;

        public double GetRoleScalingLevel(PartyData party, CrawlerUnit crawlerUnit, long roleScalingTypeId)
        {

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            long scalingLossPercent = 0;

            if (crawlerUnit.StatusEffects.HasBit(StatusEffects.Cursed))
            {
                scalingLossPercent = _gameData.Get<StatusEffectSettings>(_gs.ch).Get(StatusEffects.Cursed).Amount;
            }


            if (!crawlerUnit.IsPlayer())
            {
                double scalingPerLevel = _gameData.Get<CrawlerMonsterSettings>(_gs.ch).ScalingPerLevel;
                double unitTypeVal = crawlerUnit.Stats.Max(StatTypes.RoleScalingPercent);

                if (unitTypeVal != 0)
                {
                    scalingPerLevel += unitTypeVal / 100.0;
                }

                double totalMonsterScaling = scalingPerLevel * crawlerUnit.Level;

                totalMonsterScaling *= (100 - scalingPerLevel) / 100;

                totalMonsterScaling += combatSettings.BaseMonsterRoleScalingTier;

                return totalMonsterScaling;
            }


            List<Role> roles = roleSettings.GetRoles(crawlerUnit.Roles);

            double totalPartyMemberScaling = 0;

            double partyUpgradeScaling = _upgradeService.GetPartyBonus(party, PartyUpgrades.RoleScaling);

            double memberUpgradeScaling = _upgradeService.GetUnitBonus(crawlerUnit, EntityTypes.RoleScaling, roleScalingTypeId);
            foreach (Role role in roles)
            {
                UnitRole unitRole = crawlerUnit.Roles.FirstOrDefault(x => x.RoleId == role.IdKey);

                RoleBonusAmount bonusAmount = role.AmountBonuses.FirstOrDefault(x => x.EntityTypeId == EntityTypes.RoleScaling && x.EntityId == roleScalingTypeId);

                if (bonusAmount != null)
                {
                    totalPartyMemberScaling += unitRole.Level * (bonusAmount.Amount+partyUpgradeScaling+memberUpgradeScaling);
                }
            }

            totalPartyMemberScaling *= (100 - scalingLossPercent) / 100;
            
            totalPartyMemberScaling += combatSettings.BasePlayerRoleScalingTier;

            

            return (int)(100 * totalPartyMemberScaling) / 100.0;
        }


        public double GetSpellScalingLevel(PartyData party, CrawlerUnit unit, CrawlerSpell spell)
        {

            CrawlerSpell finalSpell = spell;

            if (finalSpell.ReplacesCrawlerSpellId > 0)
            {
                List<CrawlerSpell> spellsSeen = new List<CrawlerSpell>();
                while (finalSpell.ReplacesCrawlerSpellId > 0)
                {
                    CrawlerSpell prevSpell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(finalSpell.ReplacesCrawlerSpellId);
                  
                    if (prevSpell != null && !spellsSeen.Contains(prevSpell))
                    {
                        finalSpell = prevSpell;
                        spellsSeen.Add(prevSpell);
                    }
                    else
                    {
                        break;
                    }
                                        
                }
            }

            double scalingLevel = GetRoleScalingLevel(party, unit, finalSpell.RoleScalingTypeId);

            scalingLevel -= (finalSpell.RoleScalingTier - 1);

            return scalingLevel;
        }
    }
}
