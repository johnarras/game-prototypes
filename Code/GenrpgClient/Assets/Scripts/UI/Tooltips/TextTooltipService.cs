using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.GameObjects;
using Assets.Scripts.WorldCanvas.GameEvents;
using OxDb.SharedCore.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Tooltips
{

    public interface ITextTooltipService : IInitializable
    {

    }
    public class TextTooltipService : ITextTooltipService
    {
        protected IDispatcher _dispatcher = null;
        protected IObjectPool _objectPool = null;
        protected IClientEntityService _clientEntityService = null;

        protected CancellationToken _token;
        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<ShowTextTooltipEvent>(OnShowTextTooltipEvent, _token);
            await Task.CompletedTask;
        }

        private void OnShowTextTooltipEvent(ShowTextTooltipEvent showEvent)
        {
            if (string.IsNullOrEmpty(showEvent.Text))
            {
                return;
            }

            ShowDynamicUIItem showUIItem = new ShowDynamicUIItem
            (DynamicUILocation.ScreenSpace,
            "TextDoober",
            showEvent.Position,
            OnLoadTextDoober,
            showEvent,
            _token,
            "DynamicUI");

            _dispatcher.Dispatch(showUIItem);


        }

        private void OnLoadTextDoober(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            TextDoober td = go.GetComponent<TextDoober>();
            if (td == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            ShowTextTooltipEvent showEvent = data as ShowTextTooltipEvent;

            if (showEvent == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            td.SetText(showEvent.Text);
        }
    }
}


