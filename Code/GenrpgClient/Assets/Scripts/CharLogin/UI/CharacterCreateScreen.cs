
using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Characters.WebApi.CreateChar;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

public class CharacterCreateScreen : BaseScreen
{
    private IClientWebService _webNetworkService = null;

    public GInputField NameInput;
    public GButton CreateButton;
    public GButton BackButton;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _uiService.SetButton(CreateButton, GetName(), ClickCreate);
        _uiService.SetButton(BackButton, GetName(), ClickBack);

        await Task.CompletedTask;
    }

    public void ClickBack()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.CharacterCreate));
    }
    public void ClickCreate()
    {
        string charName = NameInput.Text;
        if (string.IsNullOrEmpty(charName))
        {
            _logService.Message("You need to choose a name!");
            return;
        }

        CreateCharRequest createCommand = new CreateCharRequest()
        {
            Name = charName,
        };

        _webNetworkService.SendWebRequest(createCommand, GetToken());

    }
}



