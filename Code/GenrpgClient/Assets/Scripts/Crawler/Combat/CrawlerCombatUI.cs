using OxDb.Client.Assets.Constants;
using OxDb.Client.Assets.Sprites.Services;
using OxDb.Client.Crawler.ClientEvents.CombatEvents;
using OxDb.Client.Crawler.UI.Units;
using OxDb.Client.Doobers.Events;
using OxDb.Client.DynamicUI.Services;
using OxDb.Client.UI.Crawler.CrawlerPanels;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Spells.Settings.Elements;
using System.Linq;
using UnityEngine;

namespace OxDb.Client.Crawler.Combat
{
    public class CrawlerCombatUI : BaseBehaviour
    {
        private ICrawlerService _crawlerService = null;
        private ISpriteService _spriteService = null;
        private IDynamicUIService _dynamicUIService = null;


        public CrawlerGroupGrid AllyGrid;
        public CrawlerGroupGrid EnemyGrid;

        private PartyStatusPanel _statusPanel;

        private GameObject GetGroupObject(string groupId)
        {
            GameObject go = AllyGrid.Icons.FirstOrDefault(x => x.Group.Id == groupId)?.gameObject ?? null;
            if (go == null)
            {
                go = EnemyGrid.Icons.FirstOrDefault(x => x.Group.Id == groupId)?.gameObject ?? null;
            }
            if (go == null)
            {
                go = _statusPanel.Rows.FirstOrDefault(x => x.GetPartyMember() != null && x.GetPartyMember().Id == groupId)?.gameObject ?? null;
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

            _spriteService.LoadAtlas(AtlasNames.CrawlerCombat, GetToken());

            _statusPanel = _clientEntityService.GetChildComponentOfParent<BaseScreen, PartyStatusPanel>(gameObject);
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


        private bool _didShowBolt = false;
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

                DooberArgs dooberArgs = _dynamicUIService.CheckoutDooberArgs();

                dooberArgs.StartPosition = startUnitUI.GetHitPosition();
                dooberArgs.EndPosition = endUnitUI.GetHitPosition();
                dooberArgs.AtlasName = AtlasNames.CrawlerCombat;
                dooberArgs.SpriteName = etype.Icon + "Bolt";
                dooberArgs.PointAtEnd = true;
                dooberArgs.LerpTime = showCombatBolt.Seconds;
                dooberArgs.StartsInUI = true;
                dooberArgs.SizeScale = showCombatBolt.SizeScale;

                _dynamicUIService.ShowDoober(dooberArgs);
            }
        }
    }
}


