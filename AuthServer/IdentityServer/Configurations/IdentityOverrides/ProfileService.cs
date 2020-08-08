using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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