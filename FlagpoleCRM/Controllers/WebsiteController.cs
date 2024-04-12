using Common.Constant;
using Common.Helper;
using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace FlagpoleCRM.Controllers
{
    public class WebsiteController : Controller
    {
        private readonly ILogger<WebsiteController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;

        public WebsiteController(ILogger<WebsiteController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiUrl = _configuration["APIUrl"];
            _superHeaderName = _configuration["SuperHeader:Name"];
            _superHeaderValue = _configuration["SuperHeader:Value"];
        }
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Request.Cookies.ContainsKey(CookiesName.JWT_COOKIE))
            {
                var token = HttpContext.Request.Cookies[CookiesName.JWT_COOKIE];
                var validateJwt = JwtHelper.ValidateToken(token, _configuration);
                if (!validateJwt.IsSuccessful)
                {
                    return Redirect("/login/authentication/index");
                }

                var userInfo = JsonConvert.DeserializeObject<JwtModel>(JsonConvert.SerializeObject(validateJwt.Data));
                TempData["email"] = userInfo.Email;
                TempData["id"] = userInfo.Id;

                var param = $"email={userInfo.Email}";
                var findAcc = await APIHelper.SearchTemplateAsync($"/api/AccountAPI/GetAccountByEmail?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var currentAcc = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(findAcc));
                TempData["avatar"] = currentAcc.Avatar;
                TempData["fullName"] = currentAcc.FullName;

                var websites = GetWebsites(userInfo.Id).Result;
                ViewBag.WebsiteCount = websites.Count;

                return View();
            }
            else
            {
                return Redirect("/login/authentication/index");
            }
        }

        public async Task<ResponseModel> CreateWebsite(string name, string accountId)
        {
            var website = new Website() { Guid = "", Url = name, AccountId = accountId };
            var response = (ResponseModel)await APIHelper.PostTemplateAsync(website, "/api/WebsiteAPI/CreateWebsite", _apiUrl, _superHeaderName, _superHeaderValue);
            if (!response.IsSuccessful)
            {
                _logger.LogError($"Create website name = {name}, accountId = {accountId} failed: {response.Message}");
            }
            return response;
        }

        public async Task<List<WebsiteDTO>> GetWebsites(string accountId)
        {
            try
            {
                var prms = $"accountId={accountId}";
                var websitesObj = await APIHelper.SearchTemplateAsync($"/api/WebsiteAPI/GetListWebsites?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
                var websites = JsonConvert.DeserializeObject<List<WebsiteDTO>>(JsonConvert.SerializeObject(websitesObj));
                return websites;
            }
            catch(Exception e)
            {
                _logger.LogError($"Get website of accountId = {accountId} failed: {e.Message}");
                return new List<WebsiteDTO>();
            }
        }
    }
}
