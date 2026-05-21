
using Google.Apis.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OxDb.SharedCore.Utils;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

public class CustomSessionOptions : AuthenticationSchemeOptions
{
    public string TokenSecret { get; set; }
}


public class CustomSessionHandler : AuthenticationHandler<CustomSessionOptions>
{
    public CustomSessionHandler(
        IOptionsMonitor<CustomSessionOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider clock) : base(options, logger, encoder)
    {

    }

    private static readonly NewtonsoftJsonSerializer serializer = new NewtonsoftJsonSerializer();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Extract the token from the Header
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        // Json of a UserSession
        var bearerToken = authorizationHeader.ToString().Replace("Bearer ", "");

        string[] dataAndHashWords = bearerToken.Split("_");

        if (dataAndHashWords.Length != 2)
        {
            return AuthenticateResult.Fail("Token Did not have proper parts");
        }

        string calcedHash = HashUtils.QuickHash(dataAndHashWords[0] + "." + Options.TokenSecret);

        if (calcedHash != dataAndHashWords[1])
        {
            return AuthenticateResult.Fail("Incoming hash did not match.");
        }

        string[] dataWords = dataAndHashWords[0].Split(".");

        if (dataWords.Length != 4)
        {
            return AuthenticateResult.Fail("4 pieces of data not sent.");
        }

        if (!long.TryParse(dataWords[2], out long ticks))
        {
            return AuthenticateResult.Fail("Third token piece was not a timestamp ticks.");
        }

        if (ticks < DateTime.UtcNow.Ticks)
        {
            return AuthenticateResult.Fail("Token has expired.");
        }

        if (string.IsNullOrEmpty(dataWords[0]))
        {
            return AuthenticateResult.Fail("UserId was blank.");
        }

        if (string.IsNullOrEmpty(dataWords[3]))
        {
            return AuthenticateResult.Fail("No Existing Data Sent");
        }

        // 4. Create the Identity
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, dataWords[0]),
            new Claim("SessionToken", bearerToken),
            new Claim("ExistingData", dataWords[3])
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
