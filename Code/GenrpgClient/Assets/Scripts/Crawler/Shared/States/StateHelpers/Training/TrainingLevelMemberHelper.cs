
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
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{
    public class TrainingLevelMemberHelper : BuildingStateHelper
    {

        ICrawlerUpgradeService _upgradeService;
        ITrainingService _trainingService = null;
        IInfoService _infoService = null;
        public override ECrawlerStates Key => ECrawlerStates.TrainingLevelMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            TrainingMemberData memberData = action.ExtraData as TrainingMemberData;

            PartyMember member = memberData.Member;

            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            TrainingInfo info = _trainingService.GetTrainingInfo(party, member);

            foreach (string msg in memberData.Messages)
            {
                stateData.AddText(_textService.HighlightText(msg + "\n\n", TextColors.ColorWhite));
            }
            memberData.Messages.Clear();

            stateData.AddText($"{member.Name}: Exp for level {member.Level + 1}: {info.TotalExp}.\nYour Exp: {member.Exp}");
            stateData.AddText($"Cost: {info.Cost} Party Gold: {info.PartyGold}");

            int maxDistinctClasses = trainingSettings.MaxDistinctClasses;

            if (maxDistinctClasses > 0)
            {
                maxDistinctClasses += (int)_upgradeService.GetPartyBonus(party, PartyUpgrades.ClassCount);
            }
            if (info.ExpLeft < 1)
            {
                if (info.PartyGold < info.Cost)
                {
                    stateData.AddText("You need more gold before you can train.");
                }
                else
                {

                    if (!trainingSettings.AdvanceOneClassPerLevel)
                    {

                        stateData.Actions.Add(new CrawlerStateAction($"Train level {member.Level + 1} for {info.Cost} Gold", 'T', ECrawlerStates.TrainingLevelMember,
                            onClickAction: delegate ()
                            {
                                _trainingService.TrainPartyMemberLevels(party, member, 0, memberData);
                            }, extraData: memberData));
                    }
                    else
                    {
                        if (maxDistinctClasses > 0)
                        {
                            stateData.AddText($"You can train up to {maxDistinctClasses} classes.");
                        }
                        

                        RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);
                        List<Role> classRoles = roleSettings.GetData().Where(x => x.RoleCategoryId == RoleCategories.Class).OrderBy(x=>x.Name).ToList();

                        List<long> allClassRoleIds = classRoles.Select(x => x.IdKey).ToList();

                        List<UnitRole> unitRoles = member.Roles.Where(x=>allClassRoleIds.Contains(x.RoleId)).ToList();  

                        List<long> myClassRoleIds = unitRoles.Select(x=>x.RoleId).ToList();  

                        foreach (Role role in classRoles)
                        {

                            if (maxDistinctClasses > 0 && unitRoles.Count >= maxDistinctClasses &&
                                !myClassRoleIds.Contains(role.IdKey))
                            {                              
                                continue;
                            }

                            UnitRole urole = member.Roles.FirstOrDefault(x => x.RoleId == role.IdKey);

                            int currLevel = (urole != null ? urole.Level : 0);
                            int nextLevel = currLevel + 1;

                            stateData.Actions.Add(new CrawlerStateAction($"Train {_infoService.CreateInfoLink(role)} to Level {nextLevel} for {info.Cost} Gold", 'T', ECrawlerStates.TrainingLevelMember,
                                onClickAction: delegate ()
                                {
                                    _trainingService.TrainPartyMemberLevels(party, member, role.IdKey, memberData);
                                }, extraData: memberData, pointerEnterAction: () => { ShowInfo(EntityTypes.Role, role.IdKey); }

                                
                                
                                ));

                        }

                    }
                }
            }
            else
            {
                stateData.AddText($"You need {info.ExpLeft} more Exp before you can level up.");
            }

            foreach (PartyMember pm in party.GetActiveParty())
            {
                if (pm != member)
                {
                    stateData.Actions.Add(new CrawlerStateAction("", (char)(pm.PartySlot + '0'), ECrawlerStates.TrainingLevelMember, extraData: new TrainingMemberData() { Member = pm }));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Back to member select", CharCodes.Escape, ECrawlerStates.TrainingLevelSelect));



            await Task.CompletedTask;
            return stateData;
        }
    }
}
