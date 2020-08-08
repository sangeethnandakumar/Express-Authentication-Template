using IdentityServer.Configurations.Configs;
using IdentityServer.Configurations.EF;
using IdentityServer.Configurations.IdentityOverrides;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
