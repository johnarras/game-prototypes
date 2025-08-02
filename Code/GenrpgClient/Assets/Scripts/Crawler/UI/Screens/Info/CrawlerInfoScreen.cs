using Assets.Scripts.Assets;
using Assets.Scripts.Info.UI;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Screens.Info
{
    public class CrawlerInfoScreen : BaseScreen
    {

        protected IInfoService _infoService;
        protected IEntityService _entityService;
        private IInputService _inputService; 
        private ILocalLoadService _localLoadService;

        public GButton OverviewButton;
        public GButton ClassButton;
        public GButton RaceButton;
        public GButton SpellButton;
        public GButton UnitsButton;
        public GButton PartyUpgradesButton;
        public GButton StatsButton;
        public GButton MemberUpgradesButton;
        public GButton StatusEffectsButton;

        public GameObject ListAnchor;

        public InfoPanel InfoPanel;


        public GText ListText;

        private List<InfoOverviewPage> _overviewPages = new List<InfoOverviewPage>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _uiService.SetButton(ClassButton, GetName(), () => { ShowRoleList(RoleCategories.Class); });
            _uiService.SetButton(RaceButton, GetName(), () => { ShowRoleList(RoleCategories.Origin); });
            _uiService.SetButton(SpellButton, GetName(), () => { ShowInfoList(EntityTypes.CrawlerSpell); });
            _uiService.SetButton(UnitsButton, GetName(), () => { ShowInfoList(EntityTypes.Unit); });
            _uiService.SetButton(PartyUpgradesButton, GetName(), () => { ShowInfoList(EntityTypes.PartyUpgrades); });
            _uiService.SetButton(MemberUpgradesButton, GetName(), () => { ShowInfoList(EntityTypes.MemberUpgrades); });
            _uiService.SetButton(StatsButton, GetName(), () => { ShowStatTypeList(); });
            _uiService.SetButton(OverviewButton, GetName(), () => { ShowOverview(); });
            _uiService.SetButton(StatusEffectsButton, GetName(), () => { ShowInfoList(EntityTypes.StatusEffect); });

            ShowOverview();
            await Task.CompletedTask;
        }

        protected override void ScreenUpdate()
        {
            base.ScreenUpdate();
            if (_inputService.ContinueKeyIsDown())
            {
                InfoPanel.PopInfoStack();
            }
        }

        private void ShowRoleList(long roleCategoryId)
        {
            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x=>x.RoleCategoryId == roleCategoryId).ToList();

            ShowChildList(roles, EntityTypes.Role);
        }

        private void ShowInfoList(long entityTypeId)
        {
            List<IIdName> children = _entityService.GetChildList(_gs.ch, entityTypeId);

            ShowChildList(children, entityTypeId);

        }

        private void ShowStatTypeList()
        {
            List<StatType> statTypes = _gameData.Get<StatSettings>(_gs.ch).GetData().Where(x => x.IsCrawlerStat).ToList();

            ShowChildList(statTypes, EntityTypes.Stat);
        }


        private void ClearAllChildren()
        {
            ClearList();
            InfoPanel.ClearInfo();
        }

        private void ClearList()
        {
            _clientEntityService.DestroyAllChildren(ListAnchor);
        }

   

        public void ShowChildList<T>(List<T> list, long entityTypeId) where T : IIdName
        {

            InfoPanel.ClearStack();

            ClearAllChildren();

            list = list.OrderBy(x => x.Name).ToList();

            foreach (IIdName idname in list)
            {
                GText text = _clientEntityService.FullInstantiate<GText>(ListText);

                _clientEntityService.AddToParent(text, ListAnchor);

                _uiService.SetText(text, idname.Name);

                _uiService.AddPointerHandlers(text, () => { InfoPanel.ShowInfo(entityTypeId, idname.IdKey); }, () => { });

            }
        }

        private void ShowOverview()
        {
            if (_overviewPages.Count < 1)
            {
                TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>("Text/Overview");

                if (!string.IsNullOrEmpty(textAsset.text))
                {
                    _infoService.SetupOverviewPages(textAsset.text);
                }

                _overviewPages = _infoService.GetOverviewPages();
            }

            InfoPanel.ClearStack();

            ClearAllChildren();

            for (int p = 0; p < _overviewPages.Count; p++)
            {
                GText text = _clientEntityService.FullInstantiate<GText>(ListText);
                _clientEntityService.AddToParent(text, ListAnchor);
                _uiService.SetText(text, _overviewPages[p].Header);

                List<string> lines = _overviewPages[p].Lines;
                _uiService.AddPointerHandlers(text, () => 
                {
                    InfoPanel.ShowLines(lines); 
                }, 
                () => { });
            }

            if (_overviewPages.Count > 0)
            {
                InfoPanel.ShowLines(_overviewPages[0].Lines);
            }
        }
    }
}
