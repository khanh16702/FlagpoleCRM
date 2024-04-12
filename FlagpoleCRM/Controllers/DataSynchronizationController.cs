using Common.Constant;
using Common.Helper;
using Common.Models;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlagpoleCRM.Controllers
{
    public class DataSynchronizationController : Controller
    {
        private readonly ILogger<DataSynchronizationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;
        public DataSynchronizationController(ILogger<DataSynchronizationController> logger, IConfiguration configuration)
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

                var prm = $"accountId={userInfo.Id}";
                var websitesObj = await APIHelper.SearchTemplateAsync($"/api/WebsiteAPI/GetListWebsites?{prm}", _apiUrl, _superHeaderName, _superHeaderValue);
                var websites = JsonConvert.DeserializeObject<List<Website>>(JsonConvert.SerializeObject(websitesObj));
                if (websites.Count == 0)
                {
                    return Redirect("/website/index");
                }
                ViewBag.WebsiteCount = websites.Count;
                return View();
            }
            else
            {
                return Redirect("/login/authentication/index");
            }
        }

        public async Task<ResponseModel> SaveAccessToken(string token, string dataSource, string shopDomain, string websiteId)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            if (string.IsNullOrEmpty(token))
            {
                response.IsSuccessful = false;
                response.Message = "Invalid access token";
                return response;
            }

            var website = new Website { Guid = websiteId, AccountId = "", Url = "", IsDeleted = false };
            switch(dataSource)
            {
                case DataSource.SHOPIFY:
                    if (string.IsNullOrEmpty(shopDomain))
                    {
                        response.IsSuccessful = false;
                        response.Message = "Shop domain could not be blank";
                        return response;
                    }

                    website.ShopifyStore = shopDomain;
                    website.ShopifyToken = token;
                    break;
                case DataSource.HARAVAN:
                    website.HaravanToken = token;
                    break;
                default:
                    response.IsSuccessful = false;
                    response.Message = "Internal server error";
                    return response;
            }
            response = (ResponseModel)await APIHelper.PostTemplateAsync(website, "/api/WebsiteAPI/UpdateWebsite", _apiUrl, _superHeaderName, _superHeaderValue);
            if (!response.IsSuccessful)
            {
                _logger.LogError($"Save access token failed with token = {token}, dataSource = {dataSource}, " +
                    $"shopDomain = {shopDomain}, websiteId = {websiteId}: {response.Message}");
            }
            return response;
        }

        public async Task<Website> GetWebsiteById(string id)
        {
            var prms = $"id={id}";
            var websiteObj = await APIHelper.SearchTemplateAsync($"/api/WebsiteAPI/GetWebsiteById?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
            if (websiteObj != null)
            {
                return JsonConvert.DeserializeObject<Website>(JsonConvert.SerializeObject(websiteObj));
            }
            _logger.LogError($"Website with id = {id} not found");
            return null;
        }
    }
}
