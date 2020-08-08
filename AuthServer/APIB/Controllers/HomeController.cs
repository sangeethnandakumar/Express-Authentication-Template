using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APIB.Models;
using System.Net.Http;
using IdentityModel.Client;

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

        public async Task<IActionResult> Privacy()
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
            var response = await apiClient.GetAsync("https://localhost:44354/Home/OpenBox");
            var content = await response.Content.ReadAsStringAsync();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
