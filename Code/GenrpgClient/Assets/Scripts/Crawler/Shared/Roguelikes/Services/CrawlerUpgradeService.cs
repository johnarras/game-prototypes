
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Crawler.Upgrades.Settings;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Crawlers.Services
{
    public interface ICrawlerUpgradeService : IInjectable
    {
        double GetPartyBonus(PartyData party, long upgradeId, int tierOverride = 0);

        bool PayForPartyUpgrade(PartyData party, long upgradeId);

        NewUpgradePointsResult GetNewPartyUpgradePoints(PartyData party, long newLevel, long upgradeReasonId, string textColor = TextColors.ColorWhite);

        bool ResetPartyUpgradePoints(PartyData party);

        long GetPartyUpgradeCost(long upgradeId, int newTier);

        double GetUnitBonus(CrawlerUnit unit, long entityTypeId, long entityId);
    }

    public class NewUpgradePointsResult
    {
        public long UpgradeReasonId { get; set; }
        public long NewLevel { get; set; }
        public long TotalUpgradePoints { get; set; }
        public long GameUpgradePoints { get; set; }
        public long RunUpgradePoints { get; set; }
        public long GameLevelsCompleted { get; set; }
        public long RunLevelsCompleted { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    public class CrawlerUpgradeService : ICrawlerUpgradeService
    {

        private IGameData _gameData;
        private IClientGameState _gs;
        private ICrawlerStatService _statService = null;
        private IDispatcher _dispatcher;
        private ITextService _textService = null;

        public double GetPartyBonus(PartyData party, long upgradeId, int tierOverride = 0)
        {
            PartyUpgrade upgradeSetting = _gameData.Get<PartyUpgradeSettings>(_gs.ch).Get(upgradeId);

            if (upgradeSetting == null)
            {
                return 0;
            }

            long finalTier = (tierOverride == 0 ? party.Upgrades[upgradeId] : tierOverride);

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
            int currTier = party.Upgrades[upgradeId];

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
            _dispatcher.Dispatch(new UpdateCrawlerUI());

            party.Upgrades[upgradeId] = nextTier;

            _statService.CalcPartyStats(party, false);

            return true;
        }

        public NewUpgradePointsResult GetNewPartyUpgradePoints(PartyData party, long newLevel, long upgradeReasonId, string textColor = TextColors.ColorWhite)
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

            UpgradeStatus status = party.UpgradeStatuses.FirstOrDefault(x => x.UpgradeReasonId == upgradeReasonId);
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
                result.RunUpgradePoints = result.TotalUpgradePoints;
            }
            else
            {
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
            }

            List<string> messages = new List<string>();

            if (result.GameUpgradePoints > 0)
            {
                messages.Add($"You gain {result.GameUpgradePoints} Upgrade Point" +
                    StrUtils.AddPluralSuffix(result.GameUpgradePoints) + " for increasing");
                messages.Add(reason.Desc + $" by {result.GameLevelsCompleted} over all runs.");
            }
            if (result.RunUpgradePoints > 0)
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

            party.UpgradePoints += result.TotalUpgradePoints;
            _dispatcher.Dispatch(new UpdateCrawlerUI());
            return result;
        }

        public bool ResetPartyUpgradePoints(PartyData party)
        {
            party.UpgradePoints = 0;
            party.Upgrades.Clear();
            party.UpgradePoints = party.TotalUpgradePoints;
            _dispatcher.Dispatch(new UpdateCrawlerUI());

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);


            foreach (PartyMember member in party.GetAllMembers())
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
                    return member.Upgrades[upgrade.IdKey] * upgrade.BonusPerTier;
                }
            }
            return 0;
        }
    }
}


