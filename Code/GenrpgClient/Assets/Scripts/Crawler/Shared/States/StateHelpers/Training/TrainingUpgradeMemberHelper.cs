
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Crawler.Upgrades.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{
    public class TrainingUpgradeMemberHelper : BuildingStateHelper
    {

        ICrawlerUpgradeService _upgradeService;
        ITrainingService _trainingService = null;
        public override ECrawlerStates Key => ECrawlerStates.TrainingUpgradeMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            TrainingMemberData memberData = action.ExtraData as TrainingMemberData;

            PartyMember member = memberData.Member;

            MemberUpgradeSettings upgradeSettings = _gameData.Get<MemberUpgradeSettings>(_gs.ch);


            foreach (string msg in memberData.Messages)
            {
                stateData.AddText(_textService.HighlightText(msg + "\n\n", TextColors.ColorWhite));
            }
            memberData.Messages.Clear();



            stateData.AddText($"{member.Name}: has {member.UpgradePoints} Upgrade Point" + StrUtils.AddPluralSuffix(member.UpgradePoints) + ".");
            stateData.AddText($"Members receive an upgrade point every {upgradeSettings.LevelsPerPoint} levels.");
            stateData.AddText($"Click a row to upgrade. (Max Tier is {upgradeSettings.MaxTier})");

            IReadOnlyList<MemberUpgrade> upgrades = upgradeSettings.GetData();

            foreach (MemberUpgrade upgrade in upgrades)
            {
                int tier = member.Upgrades.Get(upgrade.IdKey);
                double bonus = _upgradeService.GetUnitBonus(member, upgrade.EntityTypeId, upgrade.EntityId);
                stateData.Actions.Add(new CrawlerStateAction($"{upgrade.Name} Tier: {tier}/{upgradeSettings.MaxTier} Bonus:{bonus} ({upgrade.BonusPerTier}/tier)", CharCodes.None, ECrawlerStates.TrainingUpgradeMember,
                onClickAction: delegate ()
                {
                    _trainingService.TrainPartyMemberUpgrade(party, member, upgrade.IdKey, memberData);
                },
                    extraData: memberData, pointerEnterAction: () => { ShowInfo(EntityTypes.MemberUpgrades, upgrade.IdKey); }));

            }

            foreach (PartyMember pm in party.GetActiveParty())
            {
                if (pm != member)
                {
                    stateData.Actions.Add(new CrawlerStateAction("", (char)(pm.PartySlot + '0'), ECrawlerStates.TrainingUpgradeMember, extraData: new TrainingMemberData() { Member = pm }));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Back to member select", CharCodes.Escape, ECrawlerStates.TrainingUpgradeSelect));



            await Task.CompletedTask;
            return stateData;
        }
    }
}
