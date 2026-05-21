using Assets.Scripts.Accounts.Constants;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.UI.Screens;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.Signup;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SignupScreen : ErrorMessageScreen
{

    public GInputField NameInput;
    public GInputField ShareIdInput;
    public GInputField ReferrerIdInput;
    public GInputField EmailInput;
    public GInputField PasswordInput1;
    public GInputField PasswordInput2;
    public GButton LoginButton;
    public GButton SignupButton;

    protected IClientAuthService _authService = null;
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

    public void ClickLogin()
    {
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Signup));
    }

    public void ClickSignup()
    {
        ShowError("");
        string email = EmailInput.Text;
        string name = NameInput.Text;
        string password1 = PasswordInput1.Text;
        string password2 = PasswordInput2.Text;
        string shareId = ShareIdInput.Text;
        string referrerId = ReferrerIdInput.Text;
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(name) && InitClient.EditorInstance.AccountSuffixId > 0)
        {
            long id = InitClient.EditorInstance.AccountSuffixId;
            name = "john" + id;
            email = name + "@gmail.com";
            shareId = name;
            referrerId = null;
            password1 = "password";
            password2 = "password";
            InitClient.EditorInstance.AccountSuffixId++;
        }
#endif
        if (name != null)
        {
            name = name.Trim();
        }

        if (string.IsNullOrEmpty(name) ||
            name.Length < AccountConstants.MinNameLength ||
            name.Length > AccountConstants.MaxNameLength)
        {
            _logService.Info($"Your Name must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} characters.");
            return;
        }

        if (String.IsNullOrEmpty(email) || email.IndexOf("@") < 0)
        {
            _logService.Info("Email must not be blank");
            return;
        }

        if (string.IsNullOrEmpty(shareId) ||
            shareId.Length < AccountConstants.MinShareIdLength ||
            shareId.Length > AccountConstants.MaxShareIdLength)
        {
            _logService.Info($"Your ShareId must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} alphanumeric characters.");
            return;
        }

        bool allAlphanumeric = true;
        for (int s = 0; s < shareId.Length; s++)
        {
            if (!StrUtils.IsAlNum(shareId[s]))
            {
                allAlphanumeric = false;
                break;
            }
        }

        if (!allAlphanumeric)
        {
            _logService.Info($"Your ShareId must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} alphanumeric characters.");

            return;
        }

        if (password1 != password2)
        {
            _logService.Info("Passwords don't match");
            return;
        }

        if (string.IsNullOrEmpty(password1) || password1.Length < AccountConstants.MinPasswordLength)
        {
            _logService.Info($"Password must be at least {AccountConstants.MinPasswordLength} characters");
            return;
        }

        AccountSignupRequest signupCommand = new AccountSignupRequest()
        {
            Email = email,
            Password = password2,
            ShareId = shareId,
            ReferrerId = referrerId,
            Name = name,
            DeviceId = _clientCryptoService.GetDeviceId(),
        };

        _authService.SendSignupRequest(signupCommand, GetToken());

    }
}



