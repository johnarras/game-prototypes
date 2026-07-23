using OxDb.Client.Assets.Scripts.Crawler.Demo.Constants;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.StateHelpers.Training;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Crawler.Training.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Crawler.Upgrades.Settings;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Training.Services
{
    public class TrainingInfo
    {
        public long Cost { get; set; }
        public long PartyGold { get; set; }
        public long NextLevel { get; set; }
        public long TotalExp { get; set; }
        public long ExpLeft { get; set; }
        public bool ReachedLevelCap { get; set; }

        public bool CanLevelUp()
        {
            return Cost <= PartyGold && ExpLeft == 0 && !ReachedLevelCap;
        }
    }


    public class TrainingResult
    {
        public PartyMember Member { get; set; }
        public long NewUpgradePoints { get; set; }
    }

    public interface ITrainingService : IInitializable
    {
        TrainingInfo GetTrainingInfo(PartyData party, PartyMember member);
        TrainingResult TrainPartyMemberLevels(PartyData party, PartyMember member, long newRoleId, TrainingMemberData memberData = null);
        long GetLevelTrainingCost(PartyMember member);
        long GetNewClassTrainingCost(PartyMember member);
        long GetExpForNextLevel(PartyMember member);
        double GetMonsterKillExp(long level);
        double GetMonsterKillsRequired(long level);
        long GetBaseTrainingCostForNextLevel(long level);
        double GetBaseExpForNextLevel(long level);
        void TrainPartyMemberAddClass(PartyData party, PartyMember member, long classId);
        void TrainPartyMemberUpgrade(PartyData party, PartyMember member, long memberUpgradeTypeId, TrainingMemberData memberData = null);
    }

    public class TrainingService : ITrainingService
    {

        private ICrawlerStatService _statService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private IDispatcher _dispatcher = null;
        private IPartyService _partyService = null;
        private IClientConfigContainer _configContainer = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public long GetBaseTrainingCostForNextLevel(long level)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);
            return (long)(1.0 * trainingSettings.LinearCostPerLevel * (level) +
                     trainingSettings.QuadraticCostPerLevel * (level - 1) * (level - 1));
        }

        public long GetLevelTrainingCost(PartyMember member)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            long level = MathUtil.Clamp(1, member.Level, trainingSettings.MaxScalingExpLevel);

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(member.Roles);

            double goldScale = roles.Sum(x => x.TrainingGoldScale);

            if (goldScale <= 0)
            {
                goldScale = 1;
            }

            goldScale *= Math.Max(1, roles.Count(r => r.RoleCategoryId == RoleCategories.Class));

            if (trainingSettings.AdvanceOneClassPerLevel)
            {
                goldScale = 1;
            }

            return (long)Math.Ceiling(goldScale * GetBaseTrainingCostForNextLevel(level));
        }


        public long GetNewClassTrainingCost(PartyMember member)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            return GetLevelTrainingCost(member) * trainingSettings.NewClassGoldCostMult;
        }

        public double GetMonsterKillExp(long level)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);
            return trainingSettings.StartMonsterExp + trainingSettings.ExtraMonsterExp * (level - 1);
        }

        public double GetMonsterKillsRequired(long level)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);
            return trainingSettings.StartKillsNeeded + trainingSettings.ExtraKillsNeededLinear * (level - 1) +
               trainingSettings.ExtraKillsNeededQuadratic * (level - 1) * (level - 1);
        }

        public double GetBaseExpForNextLevel(long level)
        {
            return GetMonsterKillsRequired(level) * GetMonsterKillExp(level);
        }

        public long GetExpForNextLevel(PartyMember member)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            long level = MathUtil.Clamp(1, member.Level, trainingSettings.MaxScalingExpLevel);

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(member.Roles);

            double expScale = roles.Sum(x => x.TrainingXpScale);

            if (expScale <= 0)
            {
                expScale = 1;
            }

            if (trainingSettings.AdvanceOneClassPerLevel)
            {
                expScale = 1;
            }

            double baseExpToLevel = GetBaseExpForNextLevel(level);

            double totalExp = baseExpToLevel * expScale;

            totalExp *= Math.Max(1, roles.Count(r => r.RoleCategoryId == RoleCategories.Class));

            return (long)Math.Ceiling(totalExp);
        }


        public TrainingInfo GetTrainingInfo(PartyData party, PartyMember member)
        {
            CrawlerTrainingSettings settings = _gameData.Get<CrawlerTrainingSettings>(null);

            long cost = GetLevelTrainingCost(member);

            long exp = GetExpForNextLevel(member);

            bool reachedLevelCap = false;

            if (_configContainer.Config.Flags.HasFlag(ClientPlayerFlags.IsDemo) && member.Level >= DemoConstants.MaxLevel)
            {
                reachedLevelCap = false;
            }

            TrainingInfo info = new TrainingInfo()
            {
                Cost = cost,
                TotalExp = exp,
                ExpLeft = Math.Max(0, exp - member.Exp),
                PartyGold = party.Currencies[CoreCurrencyTypes.Coins],
                NextLevel = member.Level + 1,
                ReachedLevelCap = reachedLevelCap,
            };

            return info;
        }

        public TrainingResult TrainPartyMemberLevels(PartyData party, PartyMember member, long newRoleId, TrainingMemberData memberData = null)
        {
            TrainingResult result = new TrainingResult()
            {
                Member = member,
            };

            if (memberData == null)
            {
                memberData = new TrainingMemberData();
            }

            TrainingInfo info = GetTrainingInfo(party, member);

            MemberUpgradeSettings memberUpgradeSettings = _gameData.Get<MemberUpgradeSettings>(_gs.ch);

            IReadOnlyList<Role> allRoles = _gameData.Get<RoleSettings>(_gs.ch).GetData();

            if (info.Cost > party.Currencies[CoreCurrencyTypes.Coins] || info.TotalExp > member.Exp)
            {
                return result;
            }

            _partyService.AddGold(party, -info.Cost);
            _partyService.AddExp(party, member, -info.TotalExp);
            member.Level++;

            NewUpgradePointsResult levelResult = _upgradeService.GetNewPartyUpgradePoints(party, member.Level, UpgradeReasons.PartyLevel, "");

            result.NewUpgradePoints = levelResult.TotalUpgradePoints;

            memberData.Messages.AddRange(levelResult.Messages);
            if (newRoleId > 0)
            {
                UnitRole currRole = member.Roles.FirstOrDefault(x => x.RoleId == newRoleId);
                if (currRole == null)
                {
                    member.Roles.Add(new UnitRole() { RoleId = newRoleId }); // Leave level at 0 since we will advance it here.
                }

                foreach (UnitRole urole in member.Roles)
                {
                    Role role = allRoles.FirstOrDefault(x => x.IdKey == urole.RoleId);

                    if (role.RoleCategoryId != RoleCategories.Class)
                    {
                        urole.Level = (int)member.Level;
                    }
                    else if (urole.RoleId == newRoleId)
                    {
                        urole.Level++;
                    }
                }
            }
            else
            {
                foreach (UnitRole urole in member.Roles)
                {
                    urole.Level = (int)member.Level;
                }
            }

            memberData.Messages.Add($"{member.Name} reaches level {member.Level}!");

            if (memberUpgradeSettings.LevelsPerPoint > 0)
            {
                int upgradesPerTier = (int)_upgradeService.GetPartyBonus(party, PartyUpgrades.MemberUpgradePoints) + 1;

                long tiers = member.Level / memberUpgradeSettings.LevelsPerPoint;

                long totalPoints = upgradesPerTier * tiers;

                long usedPoints = member.Upgrades.Data.Sum(u => u);

                long newPoints = totalPoints - usedPoints;

                if (newPoints > 0)
                {
                    member.UpgradePoints += upgradesPerTier;

                    memberData.Messages.Add("You gain " + upgradesPerTier + " Upgrade Point" + (upgradesPerTier == 1 ? "" : "s"));
                }
            }

            GainStatsOnLevelUp(party, member, memberData);

            _statService.CalcUnitStats(party, member, true);

            return result;
        }

        private void GainStatsOnLevelUp(PartyData party, PartyMember member, TrainingMemberData memberData)
        {

            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            if (trainingSettings.StatGainOnLevelMult < 1)
            {
                return;
            }

            if (member.Level % trainingSettings.StatGainOnLevelMult == 0)
            {

                List<StatType> primaryStats = _gameData.Get<StatSettings>(null).GetData().Where(
                    x => x.IdKey >= StatConstants.PrimaryStatStart &&
                    x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

                foreach (StatType primaryStat in primaryStats)
                {
                    member.AddPermStat(primaryStat.IdKey, 1);
                }


                memberData.Messages.Add($"You gain +1 point in each primary stat!");

            }
            else
            {
                memberData.Messages.Add($"You will gain +1 to each primary stat every multiple of {trainingSettings.StatGainOnLevelMult} Levels.");
            }
        }


        public void TrainPartyMemberUpgrade(PartyData party, PartyMember member, long memberUpgradeTypeId, TrainingMemberData memberData = null)
        {
            if (member.UpgradePoints < 1)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You don't have any upgrade points!", EFloatingTextArt.Error));
                return;
            }

            MemberUpgradeSettings settings = _gameData.Get<MemberUpgradeSettings>(_gs.ch);

            MemberUpgrade upgrade = settings.Get(memberUpgradeTypeId);

            if (upgrade == null)
            {
                _dispatcher.Dispatch(new ShowFloatingText("That upgrade doesn't exist!", EFloatingTextArt.Error));
                return;
            }

            int currVal = member.Upgrades[memberUpgradeTypeId];

            if (currVal >= settings.MaxTier)
            {
                _dispatcher.Dispatch(new ShowFloatingText($"{member.Name} is already at max tier {settings.MaxTier}.", EFloatingTextArt.Error));
                return;
            }

            member.Upgrades.Add(memberUpgradeTypeId, 1);

            if (memberData != null)
            {
                memberData.Messages.Add($"Added a point to {upgrade.Name}");
            }
            member.UpgradePoints--;
        }

        public void TrainPartyMemberAddClass(PartyData party, PartyMember member, long classId)
        {
            long cost = GetNewClassTrainingCost(member);

            if (party.Currencies[CoreCurrencyTypes.Coins] < cost)
            {
                _dispatcher.Dispatch(new ShowFloatingText("Not enough gold!", EFloatingTextArt.Error));
                return;
            }

            Role role = _gameData.Get<RoleSettings>(_gs.ch).Get(classId);

            if (role == null || role.RoleCategoryId != RoleCategories.Class)
            {

                _dispatcher.Dispatch(new ShowFloatingText("That is not a valid class.", EFloatingTextArt.Error));
                return;
            }

            if (member.Roles.FastAny(x => x.RoleId == classId))
            {
                _dispatcher.Dispatch(new ShowFloatingText("You're already a member of this class", EFloatingTextArt.Error));
                return;
            }


            _partyService.AddGold(party, -cost);
            member.Roles.Add(new Units.Entities.UnitRole() { RoleId = role.IdKey, Level = 1 });

            _statService.CalcUnitStats(party, member, true);
        }

        public void TrainPartyMemberOneClass(PartyData party, PartyMember member, long roleId, TrainingMemberData memberData = null)
        {
            TrainingInfo info = GetTrainingInfo(party, member);

            if (info.Cost > party.Currencies[CoreCurrencyTypes.Coins] || member.Exp < info.TotalExp)
            {
                return;
            }

            _partyService.AddGold(party, -info.Cost);
            _partyService.AddExp(party, member, -info.TotalExp);
            member.Level++;

            foreach (UnitRole urole in member.Roles)
            {
                urole.Level = (int)member.Level;
            }

            if (memberData != null)
            {
                memberData.Messages.Add($"{member.Name} reaches level {member.Level}!");
            }

            GainStatsOnLevelUp(party, member, memberData);


            _statService.CalcUnitStats(party, member, true);
        }
    }
}


