using Common.Constant;
using Common.Extension;
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
    public class TemplateBuilderController : Controller
    {
        private readonly ILogger<TemplateBuilderController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;

        public TemplateBuilderController(ILogger<TemplateBuilderController> logger, IConfiguration configuration)
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

        public async Task<List<Template>> GetTemplates(string websiteId, string email)
        {
            try
            {
                var param = $"email={email}";
                var findAcc = await APIHelper.SearchTemplateAsync($"/api/AccountAPI/GetAccountByEmail?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (findAcc == null)
                {
                    throw new Exception("Current account not found");
                }
                var acc = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(findAcc));
                var timezone = acc.Timezone;

                var prms = $"websiteId={websiteId}";
                var templatesObj = await APIHelper.SearchTemplateAsync($"/api/TemplateBuilderAPI/GetTemplates?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
                var templates = JsonConvert.DeserializeObject<List<Template>>(JsonConvert.SerializeObject(templatesObj));

                foreach(Template template in templates)
                {
                    template.CreatedDate = template.CreatedDate.GetTimeWithOffset(timezone);
                }
                return templates;
            }
            catch (Exception e)
            {
                _logger.LogError($"Get templates of websiteId = {websiteId} failed: {e.Message}");
                return new List<Template>();
            }
        }

        public async Task<ResponseModel> GetTemplateById(int id)
        {
            try
            {
                var prms = $"id={id}";
                var templateObj = await APIHelper.SearchTemplateAsync($"/api/TemplateBuilderAPI/GetTemplateById?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (templateObj == null)
                {
                    throw new Exception("Template not found");
                }
                var template = JsonConvert.DeserializeObject<Template>(JsonConvert.SerializeObject(templateObj));
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Data = template
                };
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when trying to get template with id = {id}: {ex.Message}");
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = "Some errors occurred"
                };
            }
        }

        public async Task<ResponseModel> InsertOrUpdate(Template template)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                if (string.IsNullOrWhiteSpace(template.Name))
                {
                    throw new Exception("Name is not set yet");
                }

                var prms = $"id={template.WebsiteGuid}";
                var findWebsite = await APIHelper.SearchTemplateAsync($"/api/WebsiteAPI/GetWebsiteById?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (findWebsite == null)
                {
                    throw new Exception("Current website not found");
                }
                template.WebsiteId = JsonConvert.DeserializeObject<Website>(JsonConvert.SerializeObject(findWebsite)).Id;

                var resInsert = (ResponseModel)await APIHelper.PostTemplateAsync(template, $"/api/TemplateBuilderAPI/InsertOrUpdate", _apiUrl, _superHeaderName, _superHeaderValue);
                if (!resInsert.IsSuccessful)
                {
                    throw new Exception("Some error occurred");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when trying to insert/update template: TemplateId = {template.Id}, WebsiteId = {template.WebsiteGuid}: {ex.Message}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
