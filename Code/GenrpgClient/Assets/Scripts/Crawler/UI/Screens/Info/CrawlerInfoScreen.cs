using Assets.Scripts.Info.UI;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.Screens.Info
{
    public class CrawlerInfoScreen : BaseInfoScreen
    {
        protected ICrawlerService _crawlerService = null;
        protected ICrawlerWorldService _worldService = null;

        public GButton OverviewButton;
        public GButton ClassButton;
        public GButton RaceButton;
        public GButton SpellButton;
        public GButton UnitsButton;
        public GButton PartyUpgradesButton;
        public GButton StatsButton;
        public GButton MemberUpgradesButton;
        public GButton StatusEffectsButton;
        public GButton ElementsButton;
        public GButton MapsButton;
        public GButton ItemsButton;
        public GButton LootRanksButton;

        protected override string OverviewPath => "Text/CrawlerOverview";

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await base.OnStartOpen(data, token);
            _uiService.SetButton(ClassButton, GetName(), () => { ShowRoleList(RoleCategories.Class); });
            _uiService.SetButton(RaceButton, GetName(), () => { ShowRoleList(RoleCategories.Origin); });
            _uiService.SetButton(SpellButton, GetName(), () => { ShowInfoList(EntityTypes.CrawlerSpell); });
            _uiService.SetButton(UnitsButton, GetName(), () => { ShowInfoList(EntityTypes.Unit); });
            _uiService.SetButton(PartyUpgradesButton, GetName(), () => { ShowInfoList(EntityTypes.PartyUpgrades); });
            _uiService.SetButton(MemberUpgradesButton, GetName(), () => { ShowInfoList(EntityTypes.MemberUpgrades); });
            _uiService.SetButton(StatsButton, GetName(), () => { ShowStatTypeList(); });
            _uiService.SetButton(OverviewButton, GetName(), () => { ShowOverview(); });
            _uiService.SetButton(StatusEffectsButton, GetName(), () => { ShowInfoList(EntityTypes.StatusEffect); });
            _uiService.SetButton(ElementsButton, GetName(), () => { ShowInfoList(EntityTypes.Element); });
            _uiService.SetButton(MapsButton, GetName(), () => { ShowMapList(); });
            _uiService.SetButton(ItemsButton, GetName(), () => { ShowInfoList(EntityTypes.Item); });
            _uiService.SetButton(LootRanksButton, GetName(), () => { ShowInfoList(EntityTypes.LootRank); });
            ShowOverview();

            await Task.CompletedTask;
        }

        private void ShowRoleList(long roleCategoryId)
        {
            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.RoleCategoryId == roleCategoryId).ToList();

            ShowChildList(roles, EntityTypes.Role);
        }

        private void ShowStatTypeList()
        {
            List<StatType> statTypes = _gameData.Get<StatSettings>(_gs.ch).GetData().Where(x => x.IsCrawlerStat).ToList();

            ShowChildList(statTypes, EntityTypes.Stat);
        }

        private void ShowMapList()
        {
            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = _worldService.GetWorld(party.WorldId).Result;

            List<CrawlerMap> okMaps = world.Maps.Where(x => x.BaseCrawlerMapId == x.IdKey).ToList();

            ShowChildList(okMaps, EntityTypes.Map);

        }
    }
}


