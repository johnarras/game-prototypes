using Assets.Scripts.Accounts.Constants;
using Assets.Scripts.Auth.Services;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.UI.Screens;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Names.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SignupScreen : ErrorMessageScreen
{
    public GInputField VisibleNameInput;
    public GInputField ReferrerIdInput;
    public GInputField EmailInput;
    public GInputField PasswordInput1;
    public GInputField PasswordInput2;
    public GButton LoginButton;
    public GButton SignupButton;
    public GButton MainAuthButton;

    protected IClientAuthService _authService = null;
    protected IRepositoryService _repoService = null;
    protected IClientAppService _clientAppService = null;
    protected IClientCryptoService _clientCryptoService = null;
    protected INameValidationService _nameValidationService = null;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _uiService.SetButton(LoginButton, GetName(), ClickLogin);
        _uiService.SetButton(SignupButton, GetName(), ClickSignup);
        _uiService.SetButton(MainAuthButton, GetName(), ClickMainAuth);
        await Task.CompletedTask;
    }

    public void ClickLogin()
    {
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Signup));
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
    }

    public void ClickMainAuth()
    {
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Signup));
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen()));
    }

    public async ValueTask ClickSignup(CancellationToken token)
    {
        ShowError("");
        string email = EmailInput.Text;
        string password1 = PasswordInput1.Text;
        string password2 = PasswordInput2.Text;
        string visibleName = VisibleNameInput.Text;
        string referrerId = ReferrerIdInput.Text;
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(visibleName) && InitClient.EditorInstance.AccountSuffixId > 0)
        {
            long id = InitClient.EditorInstance.AccountSuffixId;
            visibleName = "john" + id;
            email = visibleName + "@gmail.com";
            referrerId = null;
            password1 = "password";
            password2 = "password";
            InitClient.EditorInstance.AccountSuffixId++;
        }
#endif
        if (visibleName != null)
        {
            visibleName = visibleName.Trim();
        }

        if (string.IsNullOrEmpty(visibleName) ||
            visibleName.Length < AccountConstants.MinNameLength ||
            visibleName.Length > AccountConstants.MaxNameLength)
        {
            _logService.Info($"Your Name must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} characters.");
            return;
        }

        if (String.IsNullOrEmpty(email) || email.IndexOf("@") < 0)
        {
            _logService.Info("Email must not be blank");
            return;
        }

        ValidateNameResult result = await _nameValidationService.ValidateName(visibleName);

        if (!result.Ok)
        {
            _logService.Info(result.ErrorMessage);
            return;
        }

        bool allAlphanumeric = true;
        for (int s = 0; s < visibleName.Length; s++)
        {
            if (!StrUtils.IsAlNum(visibleName[s]))
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

        AccountAuthRequest signupCommand = new AccountAuthRequest()
        {
            AuthType = EAuthTypes.Email,
            UserIdentity = email,
            UserSecret = password2,
            ReferrerId = referrerId,
        };

        _authService.SendAccountAuthRequest(signupCommand, true, GetToken());

    }
}



