using System.Security.Claims;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Models;
using ErrorOr;
using OpenIddict.Abstractions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Auth.OAuthAuthenticators;

public class DiscordOAuthAuthenticator(ILogger<DiscordOAuthAuthenticator> logger)
    : IOAuthAuthenticator
{
    public string Provider => Providers.Discord;

    private readonly ILogger<DiscordOAuthAuthenticator> _logger = logger;

    public ErrorOr<OAuthIdentity> ExtractOAuthIdentity(ClaimsPrincipal claimsPrincipal)
    {
        var discordUserId = claimsPrincipal.GetClaim(ClaimTypes.NameIdentifier);
        if (discordUserId is null)
        {
            _logger.LogWarning("Could not get email claim from discord claims principal");
            return AuthErrors.OAuthInvalid;
        }

        return new OAuthIdentity(ProviderKey: discordUserId, Email: null);
    }
}
