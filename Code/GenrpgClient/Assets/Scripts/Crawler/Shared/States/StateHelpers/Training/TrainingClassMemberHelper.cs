
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Currencies.Constants;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{

    public class TrainingMemberData
    {
        public PartyMember Member { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    public class TrainingClassMemberHelper : BuildingStateHelper
    {

        private ITrainingService _trainingService = null;
        private ICrawlerUpgradeService _upgradeService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingClassMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            TrainingMemberData memberData = action.ExtraData as TrainingMemberData;

            PartyMember member = memberData.Member;

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(member.Roles).Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();

            // Show any special messages and clear old messages.
            foreach (string msg in memberData.Messages)
            {
                stateData.AddText(_textService.HighlightText(msg + "\n\n", TextColors.ColorWhite));
            }
            memberData.Messages.Clear();

            int maxRoles = (int)(1 + _upgradeService.GetPartyBonus(party, PartyUpgrades.ClassCount));

            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
            {
                maxRoles++;
            }

            if (roles.Count >= maxRoles)
            {
                stateData.AddText($"Party members are limited to {maxRoles} class"
                +
                (maxRoles == 1 ? "" : "es") + $",\n\nso {member.Name} cannot gain any more classes.");

                stateData.Actions.Add(new CrawlerStateAction("Back to member select", Key.Escape, ECrawlerStates.TrainingClassSelect));

                return stateData;

            }

            List<long> currentClassIds = roles.Select(x => x.IdKey).ToList();

            // Get roles that are different than the player's current role.
            List<Role> possibleRoles = roleSettings.GetData().Where(x => x.RoleCategoryId == RoleCategories.Class &&
            !currentClassIds.Contains(x.IdKey)).ToList();

            long trainingCost = _trainingService.GetNewClassTrainingCost(member);


            stateData.AddText($"{member.Name}: New Class Cost: {trainingCost} Party Gold: {party.Currencies.Get(CrawlerCurrencyTypes.Gold)}");

            for (int i = 0; i < possibleRoles.Count; i++)
            {
                Role role = possibleRoles[i];
                TrainingMemberData newMemberData = new TrainingMemberData()
                {
                    Member = member,

                };
                newMemberData.Messages.Add($"You added the {role.Name} Class!");
                stateData.Actions.Add(new CrawlerStateAction($"Add Class {role.Name}", FromChar((char)(i + '0')), ECrawlerStates.TrainingClassMember,
                    onClickAction: delegate ()
                    {
                        _trainingService.TrainPartyMemberAddClass(party, member, role.IdKey);



                    }, extraData: newMemberData));
            }


            foreach (PartyMember pm in party.ActiveParty)
            {
                if (pm != member)
                {
                    stateData.Actions.Add(new CrawlerStateAction("", FromChar((char)(pm.PartySlot + '0')), ECrawlerStates.TrainingClassMember, extraData: new TrainingMemberData() { Member = pm }));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Back to member select", Key.Escape, ECrawlerStates.TrainingClassSelect));

            await Task.CompletedTask;
            return stateData;
        }
    }
}


