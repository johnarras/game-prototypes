using Assets.Scripts.Assets;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Info.UI
{
    public abstract class BaseInfoScreen : BaseScreen
    {
        protected IInfoService _infoService = null;
        protected IEntityService _entityService = null;
        protected IInputService _inputService = null;
        protected ILocalLoadService _localLoadService = null;

        public GameObject ListAnchor;
        public InfoPanel InfoPanel;
        public GText ListText;

        protected List<InfoOverviewPage> _overviewPages = new List<InfoOverviewPage>();

        protected abstract string OverviewPath { get; }

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
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
        protected void ShowInfoList(long entityTypeId)
        {
            List<IIdName> children = _entityService.GetChildList(_gs.ch, entityTypeId);

            ShowChildList(children, entityTypeId);

        }
        protected void ClearAllChildren()
        {
            ClearList();
            InfoPanel.ClearInfo();
        }

        protected void ClearList()
        {
            _clientEntityService.DestroyAllChildren(ListAnchor);
        }


        protected virtual void ShowChildList<T>(List<T> list, long entityTypeId) where T : IIdName
        {

            InfoPanel.ClearStack();

            ClearAllChildren();

            list = list.OrderBy(x => x.Name).ToList();

            foreach (IIdName idname in list)
            {
                GText text = _clientEntityService.FullInstantiate<GText>(ListText);

                _clientEntityService.AddToParent(text, ListAnchor);

                _uiService.SetText(text, idname.Name);

                _uiService.AddPointerHandlers(text, (GameObject go) => { InfoPanel.ShowInfo(entityTypeId, idname.IdKey); }, (GameObject go) => { });

            }
        }


        protected virtual void ShowOverview()
        {
            if (_overviewPages.Count < 1)
            {
                TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>(OverviewPath);

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
                _uiService.AddPointerHandlers(text, (GameObject go) =>
                {
                    InfoPanel.ShowLines(lines);
                },
                (GameObject go) => { });
            }

            if (_overviewPages.Count > 0)
            {
                InfoPanel.ShowLines(_overviewPages[0].Lines);
            }
        }
    }
}


