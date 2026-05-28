using Google.Apis.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OxDb.SharedCore.Utils;
using System;
using System.Security.Claims;
using System.Security.Cryptography; // Added for FixedTimeEquals
using System.Text;
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
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    private static readonly NewtonsoftJsonSerializer serializer = new NewtonsoftJsonSerializer();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Bypass check for anonymous endpoints
        HttpContext context = Context;
        Endpoint endpoint = context.GetEndpoint();

        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return AuthenticateResult.NoResult();
        }

        // 2. Extract authorization header
        if (!Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authorizationHeader))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        string authHeaderValue = authorizationHeader.ToString();
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid Authorization Scheme");
        }

        // Slice instead of Replace to reduce allocation overhead
        string bearerToken = authHeaderValue.Substring(7);

        int underscoreIndex = bearerToken.IndexOf('_');
        if (underscoreIndex == -1)
        {
            return AuthenticateResult.Fail("Token missing signature separator");
        }

        string payload = bearerToken.Substring(0, underscoreIndex);
        string incomingHash = bearerToken.Substring(underscoreIndex + 1);

        // 3. Constant-Time Cryptographic Validation
        string calcedHash = HashUtils.QuickHash(payload + "." + Options.TokenSecret);

        byte[] calcedBytes = Encoding.UTF8.GetBytes(calcedHash);
        byte[] incomingBytes = Encoding.UTF8.GetBytes(incomingHash);

        if (!CryptographicOperations.FixedTimeEquals(calcedBytes, incomingBytes))
        {
            return AuthenticateResult.Fail("Incoming hash did not match.");
        }

        // 4. Parse Plaintext Payload
        string[] dataWords = payload.Split('.');
        if (dataWords.Length != 4)
        {
            return AuthenticateResult.Fail("Payload structurally invalid.");
        }

        string userId = dataWords[0];
        string timestampTicksString = dataWords[2];
        string existingData = dataWords[3];

        if (string.IsNullOrEmpty(userId))
        {
            return AuthenticateResult.Fail("UserId was blank.");
        }

        if (!long.TryParse(timestampTicksString, out long ticks))
        {
            return AuthenticateResult.Fail("Invalid token timestamp.");
        }

        // 5. Expiration check
        long currentTicks = DateTime.UtcNow.Ticks;
        if (ticks < currentTicks)
        {
            return AuthenticateResult.Fail("Token has expired.");
        }

        // 6. Assemble Identity and Ticket
        Claim[] claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("SessionToken", bearerToken),
            new Claim("ExistingData", existingData)
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}