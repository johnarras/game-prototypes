
using OxDb.Client.Auth.Services;
using OxDb.Client.ClientEvents.UI;
using OxDb.Client.UI.Screens;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

public class AuthMenuScreen : ErrorMessageScreen
{

#if UNITY_ANDROID
    protected IGooglePlayAuthService _googlePlayAuthService;
#endif

    public GButton GooglePlayButton;
    public GButton FacebookButton;
    public GButton GuestButton;
    public GButton EmailButton;

    protected IClientAuthService _accountAuthService = null;
    protected IRepositoryService _repoService = null;
    protected IClientAppService _clientAppService = null;
    protected IClientCryptoService _clientCryptoService = null;
    protected IFacebookAuthService _facebookAuthService = null;

    protected CancellationToken _token;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _token = token;
        _uiService.SetButton(GuestButton, GetName(), ClickGuest);
        _uiService.SetButton(EmailButton, GetName(), ClickEmail);
        _uiService.SetButton(FacebookButton, GetName(), ClickFacebook);


#if UNITY_ANDROID
        _uiService.SetButton(GooglePlayButton, GetName(), ClickGooglePlay);
#else    
        _clientEntityService.SetActive(GooglePlayButton, false);
#endif


        await Task.CompletedTask;
    }

    public void ClickEmail()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.GetMainAuthScreen()));
    }

    public async ValueTask ClickGuest(CancellationToken token)
    {
        await _accountAuthService.StartGuestLogin(token);
    }

    public void ClickGooglePlay()
    {
#if UNITY_ANDROID
        _logService.Info("Before AuthStart " + _googlePlayAuthService);
        _googlePlayAuthService.BeginAuth(_token);
        _logService.Info("After AuthStart " + _googlePlayAuthService);
#endif
    }


    public void ClickFacebook()
    {
        _facebookAuthService.BeginAuth(_token);
    }
}



