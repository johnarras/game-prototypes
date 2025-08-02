using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Stats.Constants;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Units.Entities;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Crawler.States.StateHelpers.Training;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Crawler.Upgrades.Settings;
using Unity.Profiling;

namespace Genrpg.Shared.Crawler.Training.Services
{
    public class TrainingInfo
    {
        public long Cost { get; set; }
        public long PartyGold { get; set; }
        public long NextLevel { get; set; }
        public long TotalExp { get; set; }
        public long ExpLeft { get; set; }
    }


    public class TrainingResult
    {
        public PartyMember Member { get; set; }
        public int NewUpgradePoints { get; set; }
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
        protected IClientRandom _rand = null;
        protected IClientGameState _gs = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private IDispatcher _dispatcher = null;

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

            long level = MathUtils.Clamp(1, member.Level, trainingSettings.MaxScalingExpLevel);

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(member.Roles);

            double goldScale = roles.Sum(x => x.TrainingGoldScale);

            if (goldScale <= 0)
            {
                goldScale = 1;
            }

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
            return GetMonsterKillsRequired(level)*GetMonsterKillExp(level);  
        }

        public long GetExpForNextLevel(PartyMember member)
        {
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            long level = MathUtils.Clamp(1, member.Level, trainingSettings.MaxScalingExpLevel);

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

            return (long)Math.Ceiling(totalExp);
        }


        public TrainingInfo GetTrainingInfo(PartyData party, PartyMember member)
        {
            CrawlerTrainingSettings settings = _gameData.Get<CrawlerTrainingSettings>(null);

            long cost = GetLevelTrainingCost(member);

            long exp = GetExpForNextLevel(member);

            TrainingInfo info = new TrainingInfo()
            {
                Cost = cost,
                TotalExp = exp,
                ExpLeft = Math.Max(0, exp - member.Exp),
                PartyGold = party.Gold,
                NextLevel = member.Level + 1,
            };

            return info;
        }

        public TrainingResult TrainPartyMemberLevels(PartyData party, PartyMember member, long newRoleId, TrainingMemberData memberData = null)
        {
            TrainingResult result = new TrainingResult()
            {
                Member = member,
            };

            TrainingInfo info = GetTrainingInfo(party, member);

            MemberUpgradeSettings memberUpgradeSettings = _gameData.Get<MemberUpgradeSettings>(_gs.ch);

            IReadOnlyList<Role> allRoles = _gameData.Get<RoleSettings>(_gs.ch).GetData();

            if (info.Cost > party.Gold || info.TotalExp > member.Exp)
            {
                return result;
            }
            party.Gold -= info.Cost;
            member.Exp -= info.TotalExp;
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

                int tiers = member.Level / memberUpgradeSettings.LevelsPerPoint;

                int totalPoints = upgradesPerTier * tiers;

                int usedPoints = member.Upgrades.Data.Sum(u => u);

                int newPoints = totalPoints - usedPoints;

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

        private void GainStatsOnLevelUp(PartyData partyData, PartyMember member, TrainingMemberData memberData)
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

            int currVal = member.Upgrades.Get(memberUpgradeTypeId);

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

            if (party.Gold < cost)
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

            if (member.Roles.Any(x=>x.RoleId == classId))
            {
                _dispatcher.Dispatch(new ShowFloatingText("You're already a member of this class", EFloatingTextArt.Error));
                return;
            }


            party.Gold -= cost;
            member.Roles.Add(new Units.Entities.UnitRole() { RoleId = role.IdKey, Level=1 });

            _statService.CalcUnitStats(party, member, true);
        }

        public void TrainPartyMemberOneClass(PartyData party, PartyMember member, long roleId, TrainingMemberData memberData = null)
        {
            TrainingInfo info = GetTrainingInfo(party, member);

            if (info.Cost > party.Gold || member.Exp < info.TotalExp)
            {
                return;
            }

            party.Gold -= info.Cost;
            member.Exp -= info.TotalExp;
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
