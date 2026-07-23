
using OxDb.Client.ClientEvents.UI;
using OxDb.Client.UI.MainMenu;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.LoadSave.Constants;
using OxDb.SharedGame.LoadSave.Services;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


public class LoadSaveScreen : BaseScreen
{
    public GButton LoadButton;
    public GButton SaveButton;
    public GButton DeleteButton;
    public List<LoadSaveButton> LoadButtons = new List<LoadSaveButton>();

    private ICrawlerService _crawlerService = null;

    private ILoadSaveService _loadSaveService = null;

    private int _currSlot = 0;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));

        _uiService.SetButton(LoadButton, GetName(), OnClickLoad);
        _uiService.SetButton(SaveButton, GetName(), OnClickSave);
        _uiService.SetButton(DeleteButton, GetName(), OnClickDelete);
        RefreshButtons();
        SetSlot(1);
        await Task.CompletedTask;
    }

    private void RefreshButtons()
    {
        for (int i = LoadSaveConstants.MinSlot; i <= LoadSaveConstants.MaxSlot; i++)
        {
            LoadSaveButton button = LoadButtons[i - 1];

            PartyData playerData = _loadSaveService.LoadSlot<PartyData>(i);

            button.Init(this, i, playerData);

        }
    }

    public int GetCurrentSlot()
    {
        return _currSlot;
    }

    public void SetSlot(int slot)
    {
        _currSlot = slot;

        for (int i = LoadSaveConstants.MinSlot; i <= LoadSaveConstants.MaxSlot; i++)
        {
            LoadButtons[i - 1].SetHighlight(i == slot);
        }

        LoadButton.enabled = true;
    }

    private void OnClickLoad()
    {
        PartyData party = _crawlerService.LoadParty(_currSlot);
        if (party != null)
        {
            _crawlerService.InitPartyAfterLoad(party);

            StartClose();
        }
        return;
    }


    private void OnClickContinue()
    {

    }
    private void OnClickSave()
    {
        _crawlerService.SaveGame();
    }

    private void OnClickDelete()
    {
        _loadSaveService.Delete<PartyData>(_currSlot);
    }

    protected override void OnStartClose()
    {
        if (_screenService.GetScreen(_crawlerService.GetCrawlerScreenId()) == null)
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerMainMenu));
        }
        base.OnStartClose();
    }


}



