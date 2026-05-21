
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.UI.Screens;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.Login;
using System.Threading;
using System.Threading.Tasks;

public class LoginScreen : ErrorMessageScreen
{

    public GInputField EmailInput;
    public GInputField PasswordInput;
    public GButton LoginButton;
    public GButton SignupButton;

    protected IClientAuthService _loginService = null;
    protected IRepositoryService _repoService = null;
    protected IClientAppService _clientAppService = null;
    protected IClientCryptoService _clientCryptoService = null;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _uiService.SetButton(LoginButton, GetName(), ClickLogin);
        _uiService.SetButton(SignupButton, GetName(), ClickSignup);


        await Task.CompletedTask;
    }

    public void ClickSignup()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Signup));
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

        AccountLoginRequest loginRequest = new AccountLoginRequest()
        {
            Email = EmailInput.Text,
            Password = PasswordInput.Text,
            DeviceId = _clientCryptoService.GetDeviceId(),
        };

        _loginService.SendAccountLogin(loginRequest, GetToken());
    }
}



