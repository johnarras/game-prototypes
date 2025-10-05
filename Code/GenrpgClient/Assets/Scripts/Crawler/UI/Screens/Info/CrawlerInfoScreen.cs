using Assets.Scripts.Info.UI;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.Screens.Info
{
    public class CrawlerInfoScreen : BaseInfoScreen
    {

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

    }
}
