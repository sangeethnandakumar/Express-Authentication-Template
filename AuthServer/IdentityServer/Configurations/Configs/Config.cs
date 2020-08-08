using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Configurations.Configs
{
    public static class Config
    {

        public static IEnumerable<ApiResource> GetApiResources(IConfiguration config)
        {
            var appsettingsResources = config.GetSection("IdentityServer:Resources").Get<IEnumerable<IdentityResources>>();
            var resources = new List<ApiResource>();
            foreach (var res in appsettingsResources)
            {
                resources.Add(new ApiResource(res.Name, res.DisplayName) { Scopes = res.Scopes });
            }
            return resources;
        }

        public static IEnumerable<Client> GetApiClients(IConfiguration config)
        {
            var appsettingsClients = config.GetSection("IdentityServer:Clients").Get<IEnumerable<IdentityClient>>();
            var clients = new List<Client>();
            foreach (var client in appsettingsClients)
            {
                var grandType = GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                switch (client.GrandType)
                {
                    case "ResourceOwnerPasswordAndClientCredentials":
                        grandType = GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                        break;
                    case "ClientCredentials":
                        grandType = GrantTypes.ResourceOwnerPasswordAndClientCredentials;
                        break;
                } 
                var clientSecrets = new List<Secret>();
                foreach(var secret in client.ClientSecrets)
                {
                    clientSecrets.Add(new Secret(secret.Sha256()));
                }
                clients.Add(new Client
                {
                    ClientId = client.ClientId,
                    ClientSecrets = clientSecrets,
                    AllowedScopes = client.Scopes,
                    AllowedGrantTypes = grandType,
                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 120,
                    IdentityTokenLifetime = 120,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    SlidingRefreshTokenLifetime = 30,
                    AllowOfflineAccess = true,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AlwaysSendClientClaims = true,
                    Enabled = true,
                });
            }
            return clients;
        }

        public static IEnumerable<ApiScope> GetApiScopes(IConfiguration config)
        {
            var appsettingsScopes = config.GetSection("IdentityServer:Scopes").Get<IEnumerable<string>>();
            var scopes = new List<ApiScope>();
            foreach (var scope in appsettingsScopes)
            {
                scopes.Add(new ApiScope(scope));
            }
            return scopes;
        }

    }
}
