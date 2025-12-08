using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{
    public class SelectSpellStateHelper : BaseCombatStateHelper
    {

        protected IRoleService _roleService = null!;
        public override ECrawlerStates HelperKey => ECrawlerStates.SelectSpell;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            SelectAction data = action.ExtraData as SelectAction;

            if (data == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Cannot select a spell without a select action" };
            }

            long level = data.Member.Level;

            long currMana = data.Member.Stats.Curr(StatTypes.Mana);

            List<CrawlerSpell> spells = _crawlerSpellService.GetSpellsForMember(party, data.Member);

            for (int s = 0; s < spells.Count; s++)
            {
                CrawlerSpell spell = spells[s];

                long powerCost = _crawlerSpellService.GetPowerCost(party, data.Member, spell);
                double spellLevel = _roleService.GetSpellScalingLevel(party, data.Member, spell);

                SelectSpellAction selectSpell = new SelectSpellAction()
                {
                    Action = data,
                    Spell = spell,
                    PowerCost = powerCost,
                };

                string spellText = spell.Name + " (" + selectSpell.PowerCost + " Mana) [Power: " + spellLevel + "]";
                ECrawlerStates nextState = ECrawlerStates.OnSelectSpell;
                object extra = selectSpell;
                if (selectSpell.PowerCost > currMana)
                {
                    spellText = spell.Name + "    <color=red>(" + selectSpell.PowerCost + " Mana)</color>";
                    nextState = ECrawlerStates.None;
                    extra = null;
                }

                Key chosenKey = FromChar((char)(s < 26 ? (char)('A' + s) : (s < 36 ? (char)('0' + s - 26) : 0)));

                if (chosenKey != Key.None)
                {
                    spellText = chosenKey + " " + spellText;
                }

                stateData.Actions.Add(new CrawlerStateAction(spellText, chosenKey, nextState, extraData: extra, forceButton: false,
                    pointerEnterAction: (GameObject go) => ShowInfo(EntityTypes.CrawlerSpell, spell.IdKey)));
            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, currentData.Id, extraData: data));


            await Task.CompletedTask;

            return stateData;
        }
    }
}
