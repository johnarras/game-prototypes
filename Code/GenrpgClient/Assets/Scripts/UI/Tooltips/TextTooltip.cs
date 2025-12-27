using Assets.Scripts.ClientEvents.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Tooltips
{
    public class TextTooltip : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public string TooltipText;

        public void Show(bool visible)
        {
            if (visible)
            {
                if (!string.IsNullOrEmpty(TooltipText))
                {
                    _dispatcher.Dispatch(new ShowTextTooltipEvent()
                    {
                        Position = transform.position + new UnityEngine.Vector3(0, 20, 0),
                        Text = TooltipText,
                    });
                }
            }
            else
            {
                _dispatcher.Dispatch(new HideTextTooltipEvent());
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Show(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Show(false);
        }
    }
}


