
using OxDb.Client.UI.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Info.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using OxDb.SharedGame.Crawler.Training.Services;
using OxDb.SharedGame.Crawler.Training.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Training
{
    public class TrainingLevelMemberHelper : BuildingStateHelper
    {

        private ICrawlerUpgradeService _upgradeService = null;
        private ITrainingService _trainingService = null;
        private IInfoService _infoService = null;
        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingLevelMember;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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

                        stateData.Actions.Add(new CrawlerStateAction($"Train level {member.Level + 1} for {info.Cost} Gold", Key.T, ECrawlerStates.TrainingLevelMember,
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
                        List<Role> classRoles = roleSettings.GetData().Where(x => x.RoleCategoryId == RoleCategories.Class).OrderBy(x => x.Name).ToList();

                        List<long> allClassRoleIds = classRoles.Select(x => x.IdKey).ToList();

                        List<UnitRole> unitRoles = member.Roles.Where(x => allClassRoleIds.Contains(x.RoleId)).ToList();

                        List<long> myClassRoleIds = unitRoles.Select(x => x.RoleId).ToList();

                        foreach (Role role in classRoles)
                        {

                            if (maxDistinctClasses > 0 && unitRoles.Count >= maxDistinctClasses &&
                                !myClassRoleIds.Contains(role.IdKey))
                            {
                                continue;
                            }

                            UnitRole urole = member.Roles.FirstOrDefault(x => x.RoleId == role.IdKey);

                            long currLevel = (urole != null ? urole.Level : 0);
                            long nextLevel = currLevel + 1;

                            stateData.Actions.Add(new CrawlerStateAction($"Train {_infoService.CreateInfoLink(role)} to Level {nextLevel} for {info.Cost} Gold", Key.T, ECrawlerStates.TrainingLevelMember,
                                onClickAction: delegate ()
                                {
                                    _trainingService.TrainPartyMemberLevels(party, member, role.IdKey, memberData);
                                }, extraData: memberData, pointerEnterAction: (GameObject go) => { ShowInfo(EntityTypes.Role, role.IdKey); }



                                ));

                        }

                    }
                }
            }
            else
            {
                stateData.AddText($"You need {info.ExpLeft} more Exp before you can level up.");
            }

            foreach (PartyMember pm in party.ActiveParty)
            {
                if (pm != member)
                {
                    stateData.Actions.Add(new CrawlerStateAction("", FromChar((char)(pm.PartySlot + '0')), ECrawlerStates.TrainingLevelMember, extraData: new TrainingMemberData() { Member = pm }));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Back to member select", Key.Escape, ECrawlerStates.TrainingLevelSelect));



            await Task.CompletedTask;
            return stateData;
        }
    }
}


