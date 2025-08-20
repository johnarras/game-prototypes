using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Spells.Settings.Elements;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatUI : BaseBehaviour
    {
        private ICrawlerService _crawlerService;
        private ICrawlerMapService _crawlerMapService;

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

            ElementType etype = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(showCombatBolt.ElementTypeId);

            if (etype != null)
            {

                _dispatcher.Dispatch(new ShowDoober()
                {
                    StartPosition = startObject.transform.position,
                    EndPosition = endObject.transform.position,
                    AtlasName = AtlasNames.CrawlerCombat,
                    SpriteName = etype.Icon + "Bolt",                  
                });
            }
        }
    }
}
