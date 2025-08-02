
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Upgrades.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI.Constants;

namespace Genrpg.Shared.Crawler.Crawlers.Services
{
    public interface ICrawlerUpgradeService : IInjectable
    {
        double GetPartyBonus(PartyData party, long upgradeId, int tierOverride = 0);

        bool PayForPartyUpgrade(PartyData party, long upgradeId);

        NewUpgradePointsResult GetNewPartyUpgradePoints(PartyData party, int newLevel, long upgradeReasonId, string textColor = TextColors.ColorWhite);

        bool ResetPartyUpgradePoints(PartyData party);

        long GetPartyUpgradeCost(long upgradeId, int newTier);

        double GetUnitBonus(CrawlerUnit unit, long entityTypeId, long entityId);
    }

    public class NewUpgradePointsResult
    {
        public long UpgradeReasonId { get; set; }
        public int NewLevel { get; set; }
        public int TotalUpgradePoints { get; set; }
        public int GameUpgradePoints { get; set; }
        public int RunUpgradePoints { get; set; }
        public int GameLevelsCompleted { get; set; }
        public int RunLevelsCompleted { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    public class CrawlerUpgradeService : ICrawlerUpgradeService
    {

        private IGameData _gameData;
        private IClientGameState _gs;
        private ICrawlerStatService _statService;
        private IDispatcher _dispatcher;
        private ITextService _textService;

        public double GetPartyBonus(PartyData party, long upgradeId, int tierOverride = 0)
        {
            PartyUpgrade upgradeSetting = _gameData.Get<PartyUpgradeSettings>(_gs.ch).Get(upgradeId);

            if (upgradeSetting == null)
            {
                return 0;
            }

            long finalTier = (tierOverride == 0 ? party.Upgrades.Get(upgradeId) : tierOverride);

            return upgradeSetting.BonusPerTier * finalTier;
        }


        public long GetPartyUpgradeCost(long upgradeId, int newTier)
        {

            if (newTier < 1)
            {
                return 0;
            }

            PartyUpgradeSettings settings = _gameData.Get<PartyUpgradeSettings>(_gs.ch);

            PartyUpgrade upgrade = settings.Get(upgradeId);

            if (upgrade == null || newTier > upgrade.MaxTier)
            {
                return 0;
            }


            return upgrade.BasePointCost * newTier;

        }

        public bool PayForPartyUpgrade(PartyData party, long upgradeId)
        {
            int currTier = party.Upgrades.Get(upgradeId);

            int nextTier = currTier + 1;

            long newCost = GetPartyUpgradeCost(upgradeId, nextTier);

            if (newCost < 1)
            {
                return false;
            }

            if (newCost > party.UpgradePoints)
            {
                return false;
            }

            party.UpgradePoints -= newCost;


            party.Upgrades.Set(upgradeId, nextTier);

            _statService.CalcPartyStats(party, false);

            return true;
        }

        public NewUpgradePointsResult GetNewPartyUpgradePoints (PartyData party, int newLevel, long upgradeReasonId, string textColor = TextColors.ColorWhite)
        {
            NewUpgradePointsResult result = new NewUpgradePointsResult()
            {
                UpgradeReasonId = upgradeReasonId,
            };

            UpgradeReasonSettings upgradeReasonSettings = _gameData.Get<UpgradeReasonSettings>(_gs.ch);

            UpgradeReason reason = upgradeReasonSettings.Get(upgradeReasonId);

            if (reason == null)
            {
                return result;
            }

            UpgradeStatus status = party.UpgradeStatuses.FirstOrDefault(x=>x.UpgradeReasonId == upgradeReasonId);
            if (status == null)
            {
                status = new UpgradeStatus()
                {
                    UpgradeReasonId = upgradeReasonId,
                };
                party.UpgradeStatuses.Add(status);  
            }

            if (reason.AlwaysSingleLevel)
            {
                result.TotalUpgradePoints = reason.RunPoints + reason.GamePoints;
                return result;
            }

            if (newLevel > status.RunLevel)
            {
                result.RunLevelsCompleted = newLevel - status.RunLevel;
                result.RunUpgradePoints = result.RunLevelsCompleted * reason.RunPoints;
                result.TotalUpgradePoints += result.RunUpgradePoints;
                status.RunLevel = newLevel;

            }
            if (newLevel > status.GameLevel)
            {
                result.GameLevelsCompleted = newLevel - status.GameLevel;
                result.GameUpgradePoints = result.GameLevelsCompleted * reason.GamePoints;
                result.TotalUpgradePoints += result.GameUpgradePoints;
                status.GameLevel = newLevel;
            }

            List<string> messages = new List<string>();

            if (result.GameUpgradePoints > 0)
            {
                messages.Add($"You gain {result.GameUpgradePoints} Upgrade Point" +
                    StrUtils.AddPluralSuffix(result.GameUpgradePoints) + " for increasing");
                messages.Add(reason.Desc + $" by {result.GameLevelsCompleted} over all runs.");
            }
            if (result.RunUpgradePoints> 0)
            {
                messages.Add($"You gain {result.RunUpgradePoints} Upgrade Point" +
                    StrUtils.AddPluralSuffix(result.RunUpgradePoints) + " for increasing");
                messages.Add(reason.Desc + $" by {result.RunLevelsCompleted} this run.");
            }

            if (!string.IsNullOrEmpty(textColor))
            {
                foreach (string msg in messages)
                {
                    result.Messages.Add(_textService.HighlightText(msg, textColor));
                }
            }
            else
            {
                result.Messages = new List<string>(messages);
            }

            party.TotalUpgradePoints += result.TotalUpgradePoints;
            party.UpgradePoints += result.TotalUpgradePoints;
            return result;
        }

        public bool ResetPartyUpgradePoints(PartyData party)
        {
            party.UpgradePoints = 0;
            party.Upgrades.Clear();
            party.UpgradePoints = party.TotalUpgradePoints;


            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            foreach (PartyMember member in party.Members)
            {
                List<Role> roles = roleSettings.GetRoles(member.Roles);

                List<Role> classRoles = roles.Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();


                for (int i = 1; i < classRoles.Count; i++)
                {
                    member.Roles = member.Roles.Where(x => x.RoleId != classRoles[i].IdKey).ToList();
                }
            }

            return true;
        }

        public double GetUnitBonus(CrawlerUnit unit, long entityTypeId, long entityId)
        {
            if (unit is PartyMember member)
            {
                MemberUpgrade upgrade = _gameData.Get<MemberUpgradeSettings>(_gs.ch).Get(entityTypeId, entityId);
                if (upgrade != null)
                {
                    return member.Upgrades.Get(upgrade.IdKey) * upgrade.BonusPerTier;
                }
            }
            return 0;
        }
    }
}
