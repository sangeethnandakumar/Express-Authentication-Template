using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Configurations.Configs
{
    public class IdentityAppSettings
    {
        public List<IdentityResources> Resources { get; set; }
        public List<string> Scopes { get; set; }
        public List<IdentityClient> Clients { get; set; }
    }

    public class IdentityResources
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<string> Scopes { get; set; }
    }

    public class IdentityClient
    {
        public string Name { get; set; }
        public string ClientId { get; set; }
        public List<string> ClientSecrets { get; set; }
        public string GrandType { get; set; }
        public List<string> Scopes { get; set; }
    }
}
