using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using AuthServer.EntityFramework;
using NETCore.MailKit.Core;

namespace AuthServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signinManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Identity(string type, string username, string password)
        {
            if(type=="login")
            {
                var user = await _userManager.FindByNameAsync(username);

                if(user!=null)
                {
                    var signInResult = await _signinManager.PasswordSignInAsync(user, password, false, false);
                    if(signInResult.Succeeded)
                    {
                        var roleExist = await _roleManager.RoleExistsAsync("Admin");
                        if(roleExist)
                        {
                            await _userManager.AddToRoleAsync(user, "Admin");
                        }
                        else
                        {
                            var role = new IdentityRole();
                            role.Name = "Admin";
                            await _roleManager.CreateAsync(role);
                        }
                    }
                    else if(signInResult.IsNotAllowed)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var link = Url.Action("ConfirmEmail", "Home", new { userid = user.Id, code = code }, Request.Scheme, Request.Host.ToString());
                        var html = $"<h1><a href='{link}'>Click here</a> to activate your account<h2>";
                        await _emailService.SendAsync("sangeethnandakumarofficial@gmail.com", "Confirm You Email", html, true);
                    }
                }

            }
            else if(type == "signup")
            {
                var user = new IdentityUser
                {
                    UserName = username,
                    Email = $"{username}@netrixllc.com",
                    Id = Guid.NewGuid().ToString()
                };
                var result = await _userManager.CreateAsync(user, password);

                if(result.Succeeded)
                {

                }
            }
            else
            {
                _signinManager.SignOutAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmEmail(string userid, string code)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if(user!=null)
            {
                await _userManager.ConfirmEmailAsync(user, code);
            }
            else
            {
                return BadRequest();
            }
            return View();
        }



        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
