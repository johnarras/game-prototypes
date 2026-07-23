using OxDb.Client.Assets.Constants;
using OxDb.Client.Assets.Sprites.Services;
using OxDb.Client.Networking.Services;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Characters.WebApi.DeleteChar;
using OxDb.SharedGame.MapServer.Entities;
using System.Threading;
using UnityEngine;

public class CharacterSelectRow : BaseBehaviour
{
    private IClientWebRequestService _webNetworkService = null;
    private ISpriteService _spriteService = null;

    public GText NameText;
    public GImage CharImage;
    public GameObject PlayButtonAnchor;
    public GButton DeleteButton;

    private CharacterStub _characterStub;
    private CharacterSelectScreen _screen;

    protected CancellationToken _token;
    public void Init(CharacterStub ch, CharacterSelectScreen screen, CancellationToken token)
    {
        _screen = screen;
        _characterStub = ch;
        _token = token;
        _uiService.SetText(NameText, ch.Name);
        _uiService.SetButton(DeleteButton, screen.GetName(), ClickDelete);
        _spriteService.SetAtlasSpriteInto(AtlasNames.UI, "BGItem", CharImage, token);

        if (PlayButtonAnchor == null)
        {
            return;
        }

        foreach (MapStub stub in _gs.mapStubs)
        {
            _assetService.LoadAssetInto(PlayButtonAnchor, AssetCategoryNames.UI,
                "CharacterPlayButton", OnDownloadPlayButton, token, stub, screen.Subdirectory);
        }
    }

    public CharacterStub GetStub()
    {
        return _characterStub;
    }

    public void ClickDelete()
    {
        if (_characterStub == null)
        {
            return;
        }

        DeleteCharRequest com = new DeleteCharRequest()
        {
            CharId = _characterStub.Id,
        };

        _webNetworkService.SendMainServerRequest(com, _token);
    }

    private void OnDownloadPlayButton(GameObject go, MapStub stub, CancellationToken token)
    {
        if (stub == null)
        {
            _clientEntityService.Destroy(go);
        }

        CharacterPlayButton button = go.GetComponent<CharacterPlayButton>();
        if (button == null)
        {
            _clientEntityService.Destroy(go);
        }

        button.Init(_characterStub.Id, stub.Id, _screen);
    }
}


