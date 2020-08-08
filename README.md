## API To API Communication
API to API Connection is federated through Identity Server. We are using Client Credential bearer token authentication model for this. For this we need to create 2 API projects. Let's say API-A and API-B. Then we need to create an Identity Server to sit in the middle and federate secure access. 
> We need to communicate to an endpoint in API-B from API-A
### Configure API Project A
Create a new ASP.NET Core WebAPI Project. We call it it API-A
### Configure API Project B
Create a new ASP.NET Core WebAPI Project. We call it it API-B
### Configure Identity Server
Create a new ASP.NET Core MVC Project. We call it it Identity Server


---
# CONFIGURE IDENTITY SERVER
Create a new ASP.NET Core MVC Project. We call it it Identity Server
#### Install NuGet Packages
```text
IdentityServer4
IdentityServer4.AspNetIdentity
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.SqlServer
```
Package | Why we are using it?
------------ | -------------
IdentityServer4 | This is the core library Identity Server 4 
IdentityServer4.AspNetIdentity | There are lot of ways to store user info on our application. The secure and recomended way is to use AspNetIdentity system
Microsoft.EntityFrameworkCore | We are using EF Core 6 to access our databases
Microsoft.AspNetCore.Identity.EntityFrameworkCore | EF Core 6 support for AspNEtCore Identity
Microsoft.EntityFrameworkCore.Design | This is a design component required for EF Core 6 migrations and more
Microsoft.EntityFrameworkCore.SqlServer | EF 6 Core Support for SQL Server. We are going to store our data on an SQL Server database
#### Configure AppSettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "AllowedHosts": "*",

  "ConnectionStrings": {
    "TIS": "Server=DB_SERVER;Database=DATABASE;Trusted_Connection=True;"
  },

  "IdentityServer": {
    "Scopes": [ "ScxWebApi", "ScxWebApiDev" ],
    "Resources": [
      {
        "Name": "ScxWebApi",
        "DisplayName": "Production Web API",
        "Scopes": [ "ScxWebApi" ]
      },
      {
        "Name": "ScxWebApiDev",
        "DisplayName": "Development Web API",
        "Scopes": [ "ScxWebApiDev" ]
      }
    ],
    "Clients": [
      {
        "Name": "Postman Client",
        "ClientId": "admin",
        "ClientSecrets": [ "admin123" ],
        "Scopes": [ "ScxWebApi", "ScxWebApiDev" ],
        "GrandType": "ClientCredentials"
      },

      {
        "Name": "Mobile Client",
        "ClientId": "sangee",
        "ClientSecrets": [ "sangee123" ],
        "Scopes": [ "ScxWebApi", "ScxWebApiDev" ],
        "GrandType": "ResourceOwnerPasswordAndClientCredentials"
      }
    ]
  }

}

```
Options | Why we are using it?
------------ | -------------
ConnectionStrings | Connection String to work with EF 6 Core
IdentityServer -- Scopes | An array of all scopes (API Names) our Identity Server 4 need to handle
IdentityServer -- Resources | A list of resources (API Infos) to be configured with Identity Server 4
IdentityServer -- Clients | A list of clients and their allowed scopes and token mechanism
#### Setup Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
        {
            //Configure EF6
            services.AddDbContext<AppDbContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("TIS"));
            });

            //Configure Identity
            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequiredUniqueChars = 0;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.SignIn.RequireConfirmedEmail = false;
            })                
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            //Configure IdentityServer
            services.AddIdentityServer()
                .AddInMemoryApiResources(Config.GetApiResources(Configuration))
                .AddInMemoryClients(Config.GetApiClients(Configuration))
                .AddInMemoryApiScopes(Config.GetApiScopes(Configuration))
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<IdentityUser>()
                .AddCustomResourceOwnerPasswordValidaton();

            services.AddControllersWithViews();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            
            //Use Identity Servr
            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
```
#### Identity Server Configurations
Now we need to grab contents from AppSettings.json to be provided to Identity Server 4 in meaningfull format. Let's create a model similar to AppSettings.json provided above for parsing
```csharp
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
```
#### Identity Server Configurations II
Now let's create a class that parses AppSettings.json and exposes config endpoints to be used in Startup.cs
```csharp
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
```
#### Setup Entity Framework Core 6
Now create a DbContext class for EF 6 to operate
```csharp
namespace IdentityServer.Configurations.EF
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}

```
#### OverRide Resource Owner Password Validation Extension Methord
Now we are going to implement custom "Resource Owner Password" validatior. There we try to check if the user is logged in or not using AspNet Identity. Let's create an extention methord that can be attached to IdentityServer builder in Startup.cs file
```csharp
namespace IdentityServer.Configurations.IdentityOverrides
{
    public static class ResourceOwnerPasswordValidatonExtension
    {
        public static IIdentityServerBuilder AddCustomResourceOwnerPasswordValidaton(this IIdentityServerBuilder builder)
        {
            builder.AddProfileService<ProfileService>();
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            return builder;
        }
    }
}
```
#### Implement OverRide Resource Owner Password Validatior
Let's Implement validator and profile service used by Identity Server 4
```csharp
namespace IdentityServer.Configurations.IdentityOverrides
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ResourceOwnerPasswordValidator(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signinManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _roleManager = roleManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //Custom Validation
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user != null)
            {
                try
                {
                    var isLoggedIn = await _signinManager.PasswordSignInAsync(user, context.Password, false, lockoutOnFailure: false);
                    if (isLoggedIn.Succeeded)
                    {
                        context.Result = new GrantValidationResult(user.Id, OidcConstants.AuthenticationMethods.Password);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
```
#### Create Profile Service
We also need to create a profile service override
```csharp
namespace IdentityServer.Configurations.IdentityOverrides
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var id = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(id);
            var claims = await _userManager.GetClaimsAsync(user) as List<Claim>;
            claims.Add(new Claim("username", user.UserName));
            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            context.IsActive = user != null;
        }
    }
}
```
---
# EntityFramework 6 MIGRATION
Migration is required for persisting AspNetIdenity entries.
1. We need to install Entity Framework Core 6 first. For that run the command `dotnet tool install --global dotnet-ef`
2. Create a migration by going to the project folder and run `dotnet ef migrations add FirstMigration`
3. Wait for build to finish
4. Update database by running `dotnet ef database update`

---
# CONFIGURE API-A
Create a new ASP.NET Core API Project. We call it it APIA
#### Install NuGet Packages
```text
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNet.Identity.Core
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.SqlServer
```
### Configure AppSettings.json
Add Authority & Audiance on API
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "TIS": "Server=DESKTOP-708EN4A\\SQLEXPRESS;Database=TIS;Trusted_Connection=True;"
  },

  "Security": {
    "IdentityServer": {
      "Authority": "https://localhost:44393/",
      "Audiance": "ScxWebApi"
    }
  }
}
```
### Setup Startup.cs
Setup Startup.cs to work with Identity And EF 6
```csharp
public void ConfigureServices(IServiceCollection services)
        {
            //Configure EF6
            services.AddDbContext<AppDbContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("TIS"));
            });

            //Configure Identity
            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequiredUniqueChars = 0;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            //Identity Server Configuration
            var identityAuthority = Configuration.GetSection("Security:IdentityServer:Authority").Value;
            var identityScope = Configuration.GetSection("Security:IdentityServer:Audiance").Value;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer("Bearer", config =>
            {
                config.Authority = identityAuthority;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true
                };
                config.Audience = identityScope;
            });
            
            services.AddControllersWithViews();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
```
### Add DbContext class
Add a DBContext class to work with EF 6
*Also don't forget to add custom classes for these table declarations. They are used for EF 6 migrations and ORM mappings, LINQ and quering DB
```csharp
namespace APIA.EF
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //Required database tables can come below as DbSet<T>
    }
}
```
### Controller
Using the HttpContext you will get the logged in user's details
```csharp
namespace APIA.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> OpenBox()
        {
            // We will get all inoformations of logged in user here including claims
            var userInfo = await _userManager.GetUserAsync(HttpContext.User);
            return Ok("Yeahhh");
        }
    }
}
```
> From this implementation (`_userManager.GetUserAsync(HttpContext.User);`). You will get information about the logged in client if he iuses ResourceOwner password validaton as GrandType
---
# CONFIGURE API-B
Lets configure API-B that can be used to call API-A. Most of the configurations are same. Let's create another WebAPI project that we can call APIB
#### Install NuGet Packages
```text
Microsoft.AspNetCore.Authentication.JwtBearer
IdentityModel
```
## AppSettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "AllowedHosts": "*",

  "Security": {
    "IdentityServer": {
      "Authority": "https://localhost:44393/",
      "Audiance": "ScxWebApiDev"
    }
  }
}
```
## Startup.cs
In this API we are not using ASPNet Identity
```csharp
 public void ConfigureServices(IServiceCollection services)
        {
            //Identity Server Configuration
            var identityAuthority = Configuration.GetSection("Security:IdentityServer:Authority").Value;
            var identityScope = Configuration.GetSection("Security:IdentityServer:Audiance").Value;
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", config =>
            {
                config.Authority = identityAuthority;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true
                };
                config.Audience = identityScope;
            });

            services.AddHttpClient();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
```
### Call API-A from API-B
```csharp
namespace APIB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClient;

        public HomeController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ShowSecret()
        {
            var authClient = _httpClient.CreateClient();
            var discoveryDocument = await authClient.GetDiscoveryDocumentAsync("https://localhost:44393/");
            var tokenResponse = await authClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = "admin",
                ClientSecret = "admin123",
                Scope = "ScxWebApi"
            });
            var apiClient = _httpClient.CreateClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);
            var response = await apiClient.GetAsync("https://localhost:44354/Home/Secret");
            var content = await response.Content.ReadAsStringAsync();
            return View();
        }
    }
}
```
