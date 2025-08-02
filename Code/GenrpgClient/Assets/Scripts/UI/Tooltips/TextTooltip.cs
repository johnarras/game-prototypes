using UnityEngine;

namespace Assets.Scripts.UI.Tooltips
{
    public class TextTooltip : BaseBehaviour
    {
        public GText Text;
        public GameObject Parent;

        public void Show(bool visible)
        {
            if (Parent != null && Text != null && !string.IsNullOrEmpty(Text.text))
            {
                _clientEntityService.SetActive(Parent, visible);
            }
        }
    }
}
