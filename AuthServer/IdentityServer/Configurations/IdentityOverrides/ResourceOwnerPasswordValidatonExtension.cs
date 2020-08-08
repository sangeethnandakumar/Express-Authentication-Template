using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Configurations.IdentityOverrides
{
    public static class ResourceOwnerPasswordValidatonExtension
    {
        public static IIdentityServerBuilder AddCustomResourceOwnerPasswordValidaton(this IIdentityServerBuilder builder)
        {
            //builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.AddProfileService<ProfileService>();
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            return builder;
        }
    }
}
