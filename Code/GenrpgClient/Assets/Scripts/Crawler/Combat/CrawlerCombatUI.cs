using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.UI.Units;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Spells.Settings.Elements;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatUI : BaseBehaviour
    {
        private ICrawlerService _crawlerService;

        public CrawlerGroupGrid AllyGrid;
        public CrawlerGroupGrid EnemyGrid;

        public PartyStatusPanel StatusPanel;

        private GameObject GetGroupObject(string groupId)
        {
            GameObject go = AllyGrid.Icons.FirstOrDefault(x => x.Group.Id == groupId)?.gameObject ?? null;
            if (go == null)
            {
                go = EnemyGrid.Icons.FirstOrDefault(x => x.Group.Id == groupId)?.gameObject ?? null;
            }
            if (go == null)
            {
                go = StatusPanel.Rows.FirstOrDefault(x => x.GetPartyMember() != null && x.GetPartyMember().Id == groupId)?.gameObject ?? null;
            }

            return go;
        }

        private void OnUpdateCombatGroups(UpdateCombatGroups update)
        {
            UpdateDataInternal();
        }

        public override void Init()
        {
            _dispatcher.AddListener<UpdateCombatGroups>(OnUpdateCombatGroups, GetToken());
            _dispatcher.AddListener<ShowCombatBolt>(OnShowCombatBolt, GetToken());
        }

        private void UpdateDataInternal()
        {
            PartyData party = _crawlerService.GetParty();
            if (party.Combat == null)
            {
                AllyGrid.Clear();
                EnemyGrid.Clear();
            }
            else
            {
                AllyGrid.UpdateGroups(party.Combat.Allies);
                EnemyGrid.UpdateGroups(party.Combat.Enemies);
            }
        }

        private static bool _didShowBolt = false;
        private void OnShowCombatBolt(ShowCombatBolt showCombatBolt)
        {
            if (showCombatBolt.CasterId == showCombatBolt.TargetId)
            {
                return;
            }

            GameObject startObject = GetGroupObject(showCombatBolt.CasterId);
            GameObject endObject = GetGroupObject(showCombatBolt.TargetId);

            if (startObject == null || endObject == null)
            {
                return;
            }

            BaseUnitUI startUnitUI = _clientEntityService.GetComponent<BaseUnitUI>(startObject);
            BaseUnitUI endUnitUI = _clientEntityService.GetComponent<BaseUnitUI>(endObject);

            if (startUnitUI == null || endUnitUI == null)
            {
                return;
            }


            ElementType etype = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(showCombatBolt.ElementTypeId);

            if (etype != null && !_didShowBolt)
            {
                _dispatcher.Dispatch(new ShowDooberEvent()
                {
                    StartPosition = startUnitUI.GetHitPosition(),
                    EndPosition = endUnitUI.GetHitPosition(),
                    AtlasName = AtlasNames.CrawlerCombat,
                    SpriteName = etype.Icon + "Bolt",
                    PointAtEnd = true,
                    LerpTime = showCombatBolt.Seconds,
                    StartsInUI = true,
                    SizeScale = showCombatBolt.SizeScale,
                });
            }
        }
    }
}
