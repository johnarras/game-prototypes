
using OxDb.SharedGame.Interfaces;

namespace OxDb.Client.UI.MainMenu
{
    public class LoadSaveButton : BaseBehaviour
    {

        public GButton Button;
        public GText Slot;
        public GText Name;
        public GText UpdateTime;

        public GImage SelectedImage;

        private int _slot = 0;
        private string _saveName;

        private LoadSaveScreen _screen;

        public void Init(LoadSaveScreen screen, int slot, INamedUpdateData data)
        {
            _screen = screen;
            _slot = slot;

            if (data != null)
            {
                _saveName = data.Name;
            }

            _uiService.SetText(Slot, _slot.ToString() + ".");

            _uiService.SetText(Name, !string.IsNullOrEmpty(_saveName) ? _saveName : " -- ");

            _uiService.SetText(UpdateTime, "");

            _uiService.SetButton(Button, _screen.GetName(), OnClickButton);

            SetHighlight(false);
        }

        private void OnClickButton()
        {
            _screen.SetSlot(_slot);
        }

        public void SetHighlight(bool visible)
        {

            _clientEntityService.SetActive(SelectedImage, visible);
        }
    }
}


