
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Characters.WebApi.CreateChar;

public class CharacterCreateScreen : BaseScreen
{
    private IClientWebService _webNetworkService;
    
    public GInputField NameInput;
    public GButton CreateButton;
    public GButton BackButton;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _uiService.SetButton(CreateButton, GetName(), ClickCreate);
        _uiService.SetButton(BackButton, GetName(),ClickBack);

        await Task.CompletedTask;
    }

    public void ClickBack()
    {
        _screenService.Open(ScreenNames.CharacterSelect);
        _screenService.Close(ScreenNames.CharacterCreate);
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

        _webNetworkService.SendClientUserWebRequest(createCommand, _token);

    }
}

