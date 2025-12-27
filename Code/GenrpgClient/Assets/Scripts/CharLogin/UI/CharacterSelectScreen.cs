
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.PlayerSearch;
using Assets.Scripts.UI.Screens;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.MapServer.WebApi.LoadIntoMap;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.UI.Constants;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterSelectScreen : ErrorMessageScreen
{

#if UNITY_EDITOR
    public GButton GenWorldButton;
    public GButton TestAssetsButton;
#endif
    public GameObject CharacterGridParent;
    public GButton CreateButton;
    public GButton LogoutButton;
    public GButton QuitButton;
    public GButton CrawlerButton;
    public GText ErrorText;

    protected IZoneGenService _zoneGenService = null;
    protected IClientAuthService _loginService = null;
    protected INoiseService _noiseService = null;
    protected IInputService _inputService = null;
    protected IPlayerSearchService _playerSearchService = null;
    private IClientConfigContainer _configContainer = null;
    private IClientAppService _clientAppService = null;

    public const string CharacterRowArt = "CharacterSelectRow";

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
#if UNITY_EDITOR

        if (GenWorldButton == null)
        {
            GameObject genWorldObj = (GameObject)_clientEntityService.FindChild(entity, "GenWorldButton");
            if (genWorldObj != null)
            {
                GenWorldButton = _clientEntityService.GetComponent<GButton>(genWorldObj);
            }
        }

        _uiService.SetButton(GenWorldButton, GetName(), ClickGenerate);


        if (TestAssetsButton == null)
        {
            GameObject testsAssetsObj = (GameObject)_clientEntityService.FindChild(entity, "TestAssetsButton");
            if (testsAssetsObj != null)
            {
                TestAssetsButton = _clientEntityService.GetComponent<GButton>(testsAssetsObj);
            }
        }

        _uiService.SetButton(TestAssetsButton, GetName(), ClickTestAssets);


#endif
        _clientEntityService.DestroyAllChildren(CharacterGridParent);

        _uiService.SetButton(LogoutButton, GetName(), ClickLogout);
        _uiService.SetButton(CreateButton, GetName(), ClickCharacterCreate);
        _uiService.SetButton(QuitButton, GetName(), ClickQuit);

        SetupCharacterGrid();

        await Task.CompletedTask;
    }

    public override void ShowError(string errorMessage)
    {
        _uiService.SetText(ErrorText, errorMessage);
    }

#if UNITY_EDITOR

    private void ClickTestAssets()
    {
        TestAssetDownloads dl = new TestAssetDownloads();

        _awaitableService.ForgetAwaitable(dl.RunTests(_gs, GetToken()));
    }

    private void ClickGenerate()
    {
        if (_gs.characterStubs.Count < 1)
        {
            _dispatcher.Dispatch(new ShowFloatingText("You need at least one character to generate a map.", EFloatingTextArt.Error));
        }
        LoadIntoMapRequest lwd = new LoadIntoMapRequest()
        {
            MapId = InitClient.EditorInstance.CurrMapId,
            CharId = _gs.characterStubs.Select(x => x.Id).FirstOrDefault(),
            GenerateMap = true,
            Env = _configContainer.Config.Env,
            WorldDataEnv = _assetService.GetWorldDataEnv(),
        };
        _zoneGenService.LoadMap(lwd);
    }


    private int GetIndex(int x, int y, int noiseSize)
    {
        return x + y * noiseSize;
    }

#endif

    private void ClickCharacterCreate()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterCreate));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.CharacterSelect));

    }



    private void OnSelectChar()
    {
        CharacterStub currStub = null;

        GameObject selected = (GameObject)_uiService.GetSelected();

        CharacterSelectRow currRow = null;

        if (selected != null)
        {
            currRow = selected.GetComponent<CharacterSelectRow>();
            if (currRow != null)
            {
                currStub = currRow.GetStub();
            }
        }

    }

    private void ClickLogout()
    {
        _loginService.Logout();
    }


    public virtual void SetupCharacterGrid()
    {
        if (CharacterGridParent == null)
        {
            return;
        }

        _clientEntityService.DestroyAllChildren(CharacterGridParent);

        foreach (CharacterStub stub in _gs.characterStubs)
        {
            _assetService.LoadAssetInto(CharacterGridParent, AssetCategoryNames.UI,
                CharacterRowArt, OnLoadCharacterRow, GetToken(), stub, Subdirectory);
        }
    }

    private void OnLoadCharacterRow(GameObject go, CharacterStub ch, CancellationToken token)
    {
        if (ch == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        CharacterSelectRow charRow = go.GetComponent<CharacterSelectRow>();
        if (charRow == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }
        charRow.Init(ch, this, token);
    }

    private void ClickQuit()
    {
        _clientAppService.Quit();
    }

}



