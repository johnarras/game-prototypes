using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Interfaces
{
    public interface IUIService : IInitializable, IGameTokenService
    {
        /// <summary>
        /// Set text.
        /// </summary>
        /// <param name="gtext">IText object to set</param>
        /// <param name="txt">Text to set</param>
        /// <param name="forceUpdateMesh">Force Update Mesh: EXPENSIVE, ONLY USE IF TEXT DOESN'T SET PROPERLY AFTER ACTIVATING OBJECT!!</param>
        void SetText(IText gtext, string txt, bool forceUpdateMesh = false);
        void SetInputText(IInputField input, object obj);
        int GetIntInput(IInputField field);
        long GetSelectedIdFromName(Type iidNameType, IDropdown dropdown);
        void SetImageTexture(IRawImage image, object tex);
        void SetImageSprite(IImage image, object spr);
        void SetImageColor(IImage image, Color color);
        object GetImageTexture(IRawImage image);
        int GetImageHeight(IRawImage image);
        int GetImageWidth(IRawImage image);
        void SetUVRect(IRawImage image, float xpos, float ypos, float xsize, float ysize);
        object GetSelected();
        void SetColor(IText text, object color);
        void SetButton(IButton button, string screenName, Action action, Dictionary<string, string> extraData = null);
        void SetButton(IButton button, string screenName, Func<CancellationToken, Task> awaitableAction, Dictionary<string, string> extraData = null);
        void SetAlpha(IText text, float alpha);
        void SetAutoSizing(IText text, bool autoSizing);
        void ResizeGridLayout(IGridLayoutGroup group, float xscale, float yscale);
        void AddPointerHandlers(object view, Action enterHandler, Action exitHandler);
        void ScrollToBottom(object scrollRectObj);
        void ScrollToTop(object scrollRectObj);
        void SetTextAlignemnt(IText text, int offset); // -1,0,1= left, center, right
        void SetInteractable(IButton button, bool interactable);
        void SetAsRaycastTarget(object obj, bool isRaycastTarget);
    }
}
