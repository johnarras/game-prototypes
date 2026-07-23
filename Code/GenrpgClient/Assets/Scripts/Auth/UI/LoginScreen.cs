
using OxDb.Client.Auth.Services;
using OxDb.Client.ClientEvents.UI;
using OxDb.Client.UI.Screens;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System.Threading;
using System.Threading.Tasks;

public class LoginScreen : ErrorMessageScreen
{

    public GInputField EmailInput;
    public GInputField PasswordInput;
    public GButton LoginButton;
    public GButton SignupButton;
    public GButton MainAuthButton;

    protected IClientAuthService _accountAuthService = null;
    protected IRepositoryService _repoService = null;
    protected IClientAppService _clientAppService = null;
    protected IClientCryptoService _clientCryptoService = null;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _uiService.SetButton(LoginButton, GetName(), ClickLogin);
        _uiService.SetButton(SignupButton, GetName(), ClickSignup);
        _uiService.SetButton(MainAuthButton, GetName(), ClickMainAuth);


        await Task.CompletedTask;
    }

    public void ClickSignup()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Signup));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Login));
    }
    public void ClickMainAuth()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen()));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Login));
    }

    public void ClickLogin()
    {
        ShowError("");
        if (string.IsNullOrEmpty(EmailInput.Text))
        {
            _logService.Error("Missing email");
            return;
        }
        if (string.IsNullOrEmpty(PasswordInput.Text))
        {
            _logService.Error("Missing password");
            return;
        }

        AccountAuthRequest authRequest = new AccountAuthRequest()
        {
            AuthType = EAuthTypes.Email,
            UserIdentity = EmailInput.Text,
            UserSecret = PasswordInput.Text,
        };

        _accountAuthService.SendAccountAuthRequest(authRequest, true, GetToken());
    }
}



