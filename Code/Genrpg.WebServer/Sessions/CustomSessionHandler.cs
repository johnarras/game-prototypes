using CommunityToolkit.HighPerformance.Buffers; // For ArrayPoolBufferWriter
using Genrpg.WebServer.Handlers;
using Google.Apis.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OxDb.SharedCore.Utils;
using System;
using System.Buffers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

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

        ReadOnlySpan<char> authSpan = authHeaderValue.AsSpan();
        ReadOnlySpan<char> bearerToken = authSpan.Slice(7);

        int underscoreIndex = bearerToken.IndexOf('_');
        if (underscoreIndex == -1)
        {
            return AuthenticateResult.Fail("Token missing signature separator");
        }

        ReadOnlySpan<char> payload = bearerToken.Slice(0, underscoreIndex);
        ReadOnlySpan<char> incomingHashText = bearerToken.Slice(underscoreIndex + 1);

        // 3. Constant-Time Cryptographic Validation (Zero-Allocation)
        Span<byte> calcedBytes = stackalloc byte[16];

        // Safely combine payload and secret using ArrayPoolBufferWriter
        int combinedLength = payload.Length + 1 + Options.TokenSecret.Length;
        using (ArrayPoolBufferWriter<char> writer = new ArrayPoolBufferWriter<char>(combinedLength))
        {
            writer.Write(payload);
            writer.Write(".");
            writer.Write(Options.TokenSecret);

            // Compute the raw hash bytes directly into our stack buffer using the rented buffer view
            if (HashUtils.QuickHash(writer.WrittenSpan, calcedBytes) != 16)
            {
                return AuthenticateResult.Fail("Hash generation failed.");
            }
        }

        // Convert the hex/base64 string incoming hash into raw bytes for comparison
        int incomingByteCount = incomingHashText.Length / 2;
        Span<byte> incomingBytes = stackalloc byte[incomingByteCount];

        // Note: Using a direct string conversion helper here since Convert methods typically accept strings/spans.
        // If your incoming text is hex-encoded, use Convert.FromHexString instead.

        OperationStatus status = Convert.FromHexString(incomingHashText, incomingBytes, out int bytesConsumed, out int bytesWritten);

        if (status != OperationStatus.Done)

        {
            return AuthenticateResult.Fail("Invalid hash to byte array decoding.");
        }

        if (!CryptographicOperations.FixedTimeEquals(calcedBytes, incomingBytes))
        {
            return AuthenticateResult.Fail("Incoming hash did not match.");
        }

        // 4. Parse Plaintext Payload sequentially without arrays
        ReadOnlySpan<char> remainingPayload = payload;

        int dotIndex1 = remainingPayload.IndexOf('.');
        if (dotIndex1 == -1) return AuthenticateResult.Fail("Payload structurally invalid.");
        ReadOnlySpan<char> userId = remainingPayload.Slice(0, dotIndex1);
        remainingPayload = remainingPayload.Slice(dotIndex1 + 1);

        int dotIndex2 = remainingPayload.IndexOf('.');
        if (dotIndex2 == -1) return AuthenticateResult.Fail("Payload structurally invalid.");
        ReadOnlySpan<char> sessionRand = remainingPayload.Slice(0, dotIndex2);
        remainingPayload = remainingPayload.Slice(dotIndex2 + 1);

        int dotIndex3 = remainingPayload.IndexOf('.');
        if (dotIndex3 == -1) return AuthenticateResult.Fail("Payload structurally invalid.");
        ReadOnlySpan<char> timestampTicksString = remainingPayload.Slice(0, dotIndex3);
        ReadOnlySpan<char> existingData = remainingPayload.Slice(dotIndex3 + 1);

        if (userId.IsEmpty)
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
        Claim[] claims = new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(CustomClaimTypes.GameSessionId, sessionRand.ToString()),
            new Claim(CustomClaimTypes.ExistingData, existingData.ToString())
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}