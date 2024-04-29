using Common.Constant;
using Common.Helper;
using Common.Models;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using RestSharp;
using Humanizer.Localisation.TimeToClockNotation;
using Common.Enums;
using EnumsNET;
using System.Runtime.CompilerServices;

namespace FlagpoleCRM.Controllers
{
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;

        public ReportController(ILogger<ReportController> logger, IConfiguration configuration)
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

        public string GetTotalCustomers(string websiteId)
        {
            try
            {
                var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                var client = new RestClient(options);

                var buildedQuery = ElasticHelper.BuildQuery("", 0, "flagpolecrm.customer", websiteId.Replace("-", ""), null, client);
                var elasticQuery = ElasticHelper.GetElasticQuery(buildedQuery);
                return ElasticHelper.CountLeads(elasticQuery, "flagpolecrm.customer", client).ToString("N0");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when get total customers in WebsiteId = {websiteId}: {ex.Message}");
                return "-1";
            }
        }

        public async Task<string> GetTotalOrders(string websiteId)
        {
            try
            {
                var param = $"websiteId={websiteId}";
                var res = await APIHelper.SearchTemplateAsync($"/api/ReportAPI/GetTotalOrders?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                var totalOrders = (long)response.Data;
                return totalOrders.ToString("N0");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when get total orders of WebsiteId = {websiteId}: {ex.Message}");
                return "-1";
            }
        }

        public async Task<string> GetTotalRevenue(string websiteId)
        {
            try
            {
                var param = $"websiteId={websiteId}";
                var res = await APIHelper.SearchTemplateAsync($"/api/ReportAPI/GetTotalRevenue?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                try
                {
                    var totalRevenue = (double)response.Data;
                    return totalRevenue.ToString("N");
                }
                catch
                {
                    var totalRevenue = (long)response.Data;
                    return totalRevenue.ToString("N");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when get total orders of WebsiteId = {websiteId}: {ex.Message}");
                return "-1";
            }
        }

        public async Task<List<object>> GetRFMScores(string websiteId)
        {
            var lst = new List<object>();
            try
            {
                var indexName = "flagpolecrm.customer";
                var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                var client = new RestClient(options);
                var buildedQuery = ElasticHelper.BuildQuery("", 0, "flagpolecrm.customer", websiteId.Replace("-", ""), null, client);
                var elasticQuery = ElasticHelper.GetElasticQuery(buildedQuery);
                var totalCustomers = ElasticHelper.CountLeads(elasticQuery, "flagpolecrm.customer", client);

                var param = $"websiteId={websiteId}";
                var res = await APIHelper.SearchTemplateAsync($"/api/ReportAPI/GetRFMScore?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                var rfms = JsonConvert.DeserializeObject<List<int>>(response.Data.ToString());
                var rfmCustomers = 0;
                for (int i = 0; i < 11; i++)
                {
                    rfmCustomers += rfms[i];
                    var name = ((ERFM)(i + 1)).AsString(EnumFormat.Description);
                    var y = Math.Round((double)rfms[i] / totalCustomers, 2);
                    lst.Add(new { name, y });
                }
                var otherName = "Other";
                var otherValue = Math.Round((double)(totalCustomers - rfmCustomers) / totalCustomers, 2);
                lst.Add(new
                {
                    name = otherName,
                    y = otherValue
                });
                return lst;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when get total orders of WebsiteId = {websiteId}: {ex.Message}");
                return lst;
            }
        }

        public async Task<List<List<double>>> GetRevenue(string websiteId)
        {
            try
            {
                var param = $"websiteId={websiteId}";
                var res = await APIHelper.SearchTemplateAsync($"/api/ReportAPI/GetRevenue?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                var data = JsonConvert.DeserializeObject<List<List<double>>>(response.Data.ToString());
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when get revenue of WebsiteId = {websiteId}: {ex.Message}");
                return new List<List<double>>();
            }
        }

        public async Task<List<List<double>>> GetEmailsSent(string websiteId)
        {
            try
            {
                var param = $"websiteId={websiteId}";
                var res = await APIHelper.SearchTemplateAsync($"/api/ReportAPI/GetEmailsSent?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                var data = JsonConvert.DeserializeObject<List<List<double>>>(response.Data.ToString());
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when get revenue of WebsiteId = {websiteId}: {ex.Message}");
                return new List<List<double>>();
            }
        }
    }
}
