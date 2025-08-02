using Assets.Scripts.UI.Abstractions;

namespace Assets.Scripts.Crawler.UI.ActionUI
{
    public class ActionPanelGrid : BaseBehaviour
    {

        public GGridLayoutGroup Group;

        public void SetData (bool useSmallerButtons)
        {
            if (useSmallerButtons)
            {
                _uiService.ResizeGridLayout(Group, 2, 2);
            }
        }

        public object GetContentRoot()
        {
            return _clientEntityService.GetEntity(Group);
        }
    }
}
