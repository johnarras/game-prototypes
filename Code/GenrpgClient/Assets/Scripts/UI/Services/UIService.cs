using Assets.Scripts.Awaitables;
using Assets.Scripts.Core;
using Assets.Scripts.GameObjects;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Animations;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Assets.Scripts.UI.Pointers;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Ftue.Messages;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using Scripts.Assets.Audio.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Services
{
    public class UIService : IUIService
    {
        protected IFtueService _ftueService = null;
        protected IAudioService _audioService = null;
        protected IRealtimeNetworkService _realtimeNetworkService = null;
        protected IAnalyticsService _analyticsService = null;
        protected IGameData _gameData = null;
        protected IClientRandom _rand = null;
        protected IClientGameState _gs = null;
        protected IClientEntityService _clientEntityService = null;
        protected IEntityService _entityService = null;
        private ILogService _logService = null;
        private CancellationToken _token;
        protected IAwaitableService _awaitableService = null;
        private ITextService _textService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void SetGameToken(CancellationToken token)
        {
            _token = token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itext">The text field to use.</param>
        /// <param name="txt">The text to set.</param>
        /// <param name="forceUpdateMesh">Force update Mesh ****EXPENSIVE!!!! USE SPARINGLY ONLY IF TEXT SET AFTER ACTIVATION FAILS!</param>
        public void SetText(IText itext, string txt, bool forceUpdateMesh = false)
        {
            if (itext is GText gtext)
            {
                gtext.SetText(txt);
                if (forceUpdateMesh)
                {
                    gtext.ForceMeshUpdate();
                }
            }
        }

        public void SetInputText(IInputField iInput, object obj)
        {
            if (obj != null && iInput is GInputField ginput)
            {
                ginput.text = obj.ToString();
            }
        }

        public int GetIntInput(IInputField iinput)
        {
            if (iinput is GInputField ginput)
            {
                if (Int32.TryParse(ginput.text, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public long GetSelectedIdFromName(Type iidNameType, IDropdown idropdown)
        {
            if (!(idropdown is GDropdown gdropdown))
            {
                return 0;
            }

            List<IIdName> items = _entityService.GetChildList(_gs.ch, iidNameType.Name);

            string selectedText = gdropdown.captionText.text;

            IIdName selectedItem = items.FirstOrDefault(x => x.Name == gdropdown.captionText.text);

            return selectedItem?.IdKey ?? 0;

        }

        public void SetInteractable(IButton button, bool interactable)
        {
            if (button is GButton gbutton)
            {
                gbutton.interactable = interactable;
            }
        }

        public void SetImageTexture(IRawImage rawImage, object texObj)
        {
            if (rawImage is GRawImage gRawImage)
            {
                if (texObj is Texture tex)
                {
                    gRawImage.texture = tex;
                }
                else if (texObj == null)
                {
                    gRawImage.texture = null;
                }
            }
        }

        public object GetSelected()
        {
            if (EventSystem.current == null)
            {
                return null;
            }
            return EventSystem.current.currentSelectedGameObject;
        }


        public void SetColor(IText text, object colorObj)
        {
            if (text is GText gtext && colorObj is UnityEngine.Color color)
            {
                gtext.color = color;
            }
        }

        public void SetButton(IButton button, string screenName, Action action, Dictionary<string, string> extraData = null)
        {
            if (button is GButton gbutton)
            {
                gbutton.onClick.RemoveAllListeners();
                gbutton.onClick.AddListener(
                   () =>
                   {
                       _awaitableService.ForgetAwaitable(InnerButtonClick(gbutton, screenName, action, null, extraData));

                   });
                _clientEntityService.RegisterDestroyCallback(gbutton, () => { gbutton.onClick.RemoveAllListeners(); });
                HighlightHotkey(gbutton, action);
            }
        }

        public void SetButton(IButton button, string screenName, Func<CancellationToken, Awaitable> awaitableAction, Dictionary<string, string> extraData = null)
        {
            if (button is GButton gbutton)
            {
                gbutton.onClick.RemoveAllListeners();
                gbutton.onClick.AddListener(
                   () =>
                   {
                       _awaitableService.ForgetAwaitable(InnerButtonClick(gbutton, screenName, null, awaitableAction, extraData));

                   });
                _clientEntityService.RegisterDestroyCallback(gbutton, () => { gbutton.onClick.RemoveAllListeners(); });
                HighlightHotkey(gbutton, null);
            }
        }

        private void HighlightHotkey(GButton button, Action action)
        {
            ButtonKeyListener listener = _clientEntityService.GetComponent<ButtonKeyListener>(button);

            if (listener == null || listener.Key == Key.None)
            {
                return;
            }
            listener.SetClickAction(action);
            char capitalizeLetter = (char)listener.Key;
            List<GText> gts = _clientEntityService.GetComponents<GText>(button);

            foreach (GText gt in gts)
            {
                StringBuilder sb = new StringBuilder();

                string txt = gt.text;

                for (int i = 0; i < txt.Length; i++)
                {
                    if (txt[i] == capitalizeLetter)
                    {
                        sb.Append(_textService.HighlightText(capitalizeLetter.ToString(), TextColors.ColorYellow));
                    }
                    else
                    {
                        sb.Append(txt[i]);
                    }
                }
                gt.text = sb.ToString();
            }

        }

        private int _blockButtonCount = 0;
        private async Awaitable InnerButtonClick(GButton button, string screenName, Action action, Func<CancellationToken, Awaitable> awaitableAction, Dictionary<string, string> extraData = null)
        {
            if (_blockButtonCount > 0)
            {
                return;
            }
            try
            {
                IncrementButtonBlock();
                if (button != null)
                {
                    button.interactable = false;
                }
                if (_ftueService.IsComplete(_rand, _gs.ch))
                {
                    _analyticsService.Send(AnalyticsEvents.ClickButton, button.name, screenName, extraData);
                    _audioService.PlaySound(AudioList.ButtonClick);
                    if (action != null)
                    {
                        action();
                    }
                    else if (awaitableAction != null)
                    {
                        await awaitableAction(_token);
                    }
                }
                else
                {
                    FtueStep step = _ftueService.GetCurrentStep(_rand, _gs.ch);
                    if (_ftueService.CanClickButton(_rand, _gs.ch, screenName, button.name))
                    {
                        _audioService.PlaySound(AudioList.ButtonClick);
                        _analyticsService.Send(AnalyticsEvents.ClickButton, button.name, screenName, extraData);
                        if (action != null)
                        {
                            action();
                        }
                        else if (awaitableAction != null)
                        {
                            await awaitableAction(_token);
                        }
                        if (step != null)
                        {
                            _ftueService.CompleteStep(_rand, _gs.ch, step.IdKey);
                            _realtimeNetworkService.SendMapMessage(new CompleteFtueStepMessage() { FtueStepId = step.IdKey });
                        }
                    }
                    else
                    {
                        _audioService.PlaySound(AudioList.ErrorClick);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "ButtonClick");
            }
            finally
            {
                if (button != null)
                {
                    button.interactable = true;
                }
                DecrementButtonBlock();
            }
        }

        public void IncrementButtonBlock()
        {
            _blockButtonCount++;
        }

        public void DecrementButtonBlock()
        {
            if (_blockButtonCount > 0)
            {
                _blockButtonCount--;
            }
        }

        public void ClearButtonBlock()
        {
            _blockButtonCount = 0;
        }

        public void SetAlpha(IText text, float alpha)
        {
            if (text is GText gText)
            {
                gText.alpha = alpha;
            }
        }

        public void SetAutoSizing(IText text, bool autoScaling)
        {
            if (text is GText gtext)
            {
                gtext.enableAutoSizing = autoScaling;
            }
        }

        public void ResizeGridLayout(IGridLayoutGroup group, float xscale, float yscale)
        {
            if (group is GGridLayoutGroup ggroup)
            {
                ggroup.constraintCount = (int)(ggroup.constraintCount / xscale);
                ggroup.cellSize = new Vector2(ggroup.cellSize.x * xscale, ggroup.cellSize.y * yscale);
            }
        }

        public void AddPointerHandlers(object view, Action<GameObject> enterHandler, Action<GameObject> exitHandler)
        {
            if (view is GameObject go)
            {
                view = go.GetComponent<MonoBehaviour>();
            }

            if (view is MonoBehaviour mb)
            {
                PointerHandler ph = _clientEntityService.GetOrAddComponent<PointerHandler>(mb.gameObject);
                ph.SetEnterExitHandlers(enterHandler, exitHandler);
            }
        }

        public void ScrollToBottom(object scrollRectObj)
        {
            if (scrollRectObj is GScrollRect scrollRect)
            {
                scrollRect.normalizedPosition = new Vector2(0, 0);
            }
        }
        public void ScrollToTop(object scrollRectObj)
        {
            if (scrollRectObj is ScrollRect scrollRect)
            {
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        public void SetTextAlignemnt(IText text, int offset)
        {
            if (text is GText gtext)
            {
                gtext.alignment = (offset < 0 ? TMPro.TextAlignmentOptions.Left : offset > 0 ? TMPro.TextAlignmentOptions.Right : TMPro.TextAlignmentOptions.Center);
            }
        }

        public object GetImageTexture(IRawImage rawImage)
        {
            if (rawImage is GRawImage gRawImage)
            {
                return gRawImage.texture;
            }
            return null;
        }

        public int GetImageHeight(IRawImage rawImage)
        {
            if (rawImage is GRawImage gRawImage)
            {
                if (gRawImage.texture != null)
                {
                    return gRawImage.texture.height;
                }
            }
            return 0;
        }

        public int GetImageWidth(IRawImage rawImage)
        {
            if (rawImage is GRawImage gRawImage)
            {
                if (gRawImage.texture != null)
                {
                    return gRawImage.texture.width;
                }
            }
            return 0;
        }

        public void SetUVRect(IRawImage rawImage, float xpos, float ypos, float xsize, float ysize)
        {
            if (rawImage is GRawImage gRawImage)
            {
                gRawImage.uvRect = new UnityEngine.Rect(new Vector2(xpos, ypos), new Vector2(xsize, ysize));
            }
        }

        public void SetAsRaycastTarget(object obj, bool isRaycastTarget)
        {
            List<Component> comps = _clientEntityService.GetComponents<Component>(obj);

            foreach (Component comp in comps)
            {
                if (comp is MaskableGraphic graphic)
                {
                    graphic.raycastTarget = isRaycastTarget;
                }
            }
        }

        public void ClearButton(IButton button)
        {
            if (button is GButton gbutton)
            {
                gbutton.onClick.RemoveAllListeners();
                gbutton.SetDestroyCallback(null);
            }
        }

        public void SetToggle(GToggle gToggle, UnityAction<bool> listener)
        {
            if (gToggle.Toggle == null)
            {
                return;
            }
            gToggle.Toggle.onValueChanged.RemoveAllListeners();
            gToggle.Toggle.onValueChanged.AddListener(listener);
            _clientEntityService.RegisterDestroyCallback(gToggle, () => gToggle.Toggle?.onValueChanged.RemoveAllListeners());
        }

        public void SetSlider(GSlider slider, float minValueIn, float maxValueIn, float currValue, UnityAction<float> valueChangedEvent)
        {
            if (slider == null)
            {
                return;
            }
            slider.minValue = minValueIn;
            slider.maxValue = maxValueIn;
            slider.value = currValue;

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(valueChangedEvent);
            _clientEntityService.RegisterDestroyCallback(slider, () => { slider.onValueChanged.RemoveAllListeners(); });
        }

        public void PlaceChildInParentRect(RectTransform childRect, RectTransform parentRect, float xpct, float ypct)
        {
            Vector2 anchor = new Vector2(xpct, ypct);

            if (childRect.transform.parent != parentRect.transform)
            {
                _clientEntityService.AddToParent(childRect.gameObject, parentRect.gameObject);
            }
            childRect.anchorMin = anchor;
            childRect.anchorMax = anchor;
            childRect.anchoredPosition = Vector2.zero;
        }
    }
}


