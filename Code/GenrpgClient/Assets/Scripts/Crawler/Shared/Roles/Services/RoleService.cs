using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Monsters.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.UnitEffects.Settings;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Roles.Services
{

    public interface IRoleService : IInjectable
    {

        double GetRoleScalingLevel(PartyData party, CrawlerUnit crawlerUnit, long roleScalingTypeId);
        double GetSpellScalingLevel(PartyData party, CrawlerUnit crawlerUnit, CrawlerSpell spell, bool includeSpellAttackScaling);

    }

    public class RoleService : IRoleService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;

        private ICrawlerUpgradeService _upgradeService = null;

        public double GetRoleScalingLevel(PartyData party, CrawlerUnit crawlerUnit, long roleScalingTypeId)
        {

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            long scalingLossPercent = 0;

            if (crawlerUnit.StatusEffects.HasBitIndex(StatusEffects.Cursed))
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
                    totalPartyMemberScaling += unitRole.Level * (bonusAmount.Amount);
                }
            }

            totalPartyMemberScaling += crawlerUnit.Level * (partyUpgradeScaling + memberUpgradeScaling);

            totalPartyMemberScaling *= (100 - scalingLossPercent) / 100;

            totalPartyMemberScaling += combatSettings.BasePlayerRoleScalingTier;


            return (int)(100 * totalPartyMemberScaling) / 100.0;
        }


        public double GetSpellScalingLevel(PartyData party, CrawlerUnit unit, CrawlerSpell spell, bool includeSpellAttackScaling)
        {

            CrawlerSpell finalSpell = spell;


            double scalingLevel = GetRoleScalingLevel(party, unit, finalSpell.RoleScalingTypeId);

            //scalingLevel -= (finalSpell.RoleScalingTier - 1);

            if (unit.IsPlayer())
            {
                CombatAction action = _gameData.Get<CombatActionSettings>(_gs.ch).Get(spell.CombatActionId);

                scalingLevel += action.BaseBonusHits;
            }

            if (includeSpellAttackScaling && spell.AttackQuantityScale > 0)
            {
                scalingLevel *= spell.AttackQuantityScale;
            }

            return scalingLevel;
        }
    }
}


