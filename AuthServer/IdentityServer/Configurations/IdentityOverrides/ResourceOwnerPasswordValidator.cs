using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
