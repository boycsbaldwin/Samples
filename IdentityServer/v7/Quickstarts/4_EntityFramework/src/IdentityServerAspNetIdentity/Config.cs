using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources
    {
        get
        {
            var profile = new IdentityResources.Profile();
            profile.UserClaims.Add("favorite_color");
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                profile,
                new IdentityResource("onboarding", "Onboarding", new[] { "onboarding" })
            };
        }
    }

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("api1")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "scope1" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "originalweb",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5002/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "api1", "onboarding" }
            },
            new Client
            {
                ClientId = "web",
//                ClientSecrets = { new Secret("secret".Sha256()) },
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:4444" },//"/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:4444/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:4444/signout-callback-oidc" },
                AllowedCorsOrigins = { "https://localhost:4444" },

                AllowOfflineAccess = true,
                AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "api1", "onboarding" }
            },
        };
}
