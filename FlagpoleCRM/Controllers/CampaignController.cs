using Common.Constant;
using Common.Enums;
using Common.Extension;
using Common.Helper;
using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlagpoleCRM.Controllers
{
    public class CampaignController : Controller
    {
        private readonly ILogger<CampaignController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;

        public CampaignController(ILogger<CampaignController> logger, IConfiguration configuration)
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
                TempData["timezone"] = currentAcc.Timezone;

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

        public async Task<ResponseModel> GetEmailSenderById(int id)
        {
            try
            {
                var param = $"id={id}";
                var res = await APIHelper.SearchTemplateAsync($"/api/CampaignAPI/GetEmailSenderById?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (res == null)
                {
                    throw new Exception("Email sender not found");
                }
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Data = JsonConvert.DeserializeObject<EmailAccount>(JsonConvert.SerializeObject(res))
                };
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when trying to get Email Sender Id = {id}: {ex.Message}");
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<List<EmailAccount>> GetListEmailSender(string accountId, string timezone, string websiteId)
        {
            try
            {
                var prms = $"websiteId={websiteId}";
                var res = await APIHelper.SearchTemplateAsync($"/api/CampaignAPI/GetListEmailSender?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (res == null)
                {
                    return new List<EmailAccount>();
                }
                var emailSenders = JsonConvert.DeserializeObject<List<EmailAccount>>(JsonConvert.SerializeObject(res));
                foreach(EmailAccount emailSender in emailSenders)
                {
                    emailSender.CreatedDate = emailSender.CreatedDate.Value.GetTimeWithOffset(timezone);
                }
                return emailSenders;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when trying to get list Email Senders of accountId = {accountId}: {ex.Message}");
                return new List<EmailAccount>();
            }
        }

        public async Task<ResponseModel> UpdateEmailSender(EmailAccount model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                {
                    return new ResponseModel { IsSuccessful = false, Message = "Email or password cannot be left blank" };
                }
                var res = await APIHelper.PostTemplateAsync(model, $"/api/CampaignAPI/UpdateEmailSender", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when trying to update EmaiSender Id = {model.Id}, WebsiteId = {model.WebsiteId}: {ex.Message}");
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = "Some errors occurred"
                };
            }
            
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult LoadManageEmailSenders(string accountId, string websiteId, string timezone)
        {
            var model = new
            {
                AccountId = accountId,
                WebsiteId = websiteId,
                Timezone = timezone
            };
            return PartialView("/Views/Shared/Campaign/_CampaignManageSenders.cshtml", model);
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult LoadModifyCampaignView(CampaignDTO campaign)
        {
            if (campaign.SendDate.HasValue)
            {
                campaign.SendDate = campaign.SendDate.Value.GetTimeWithOffset(campaign.Timezone);
                campaign.SendDateAtInput = campaign.SendDate.Value.ToString("yyyy-MM-ddTHH:mm");
            }
            return PartialView("/Views/Shared/Campaign/_CampaignModify.cshtml", campaign);
        }

        public async Task<List<Campaign>> GetListCampaign(string timezone, string websiteId)
        {
            var prms = $"websiteId={websiteId}";
            var res = await APIHelper.SearchTemplateAsync($"/api/CampaignAPI/GetListCampaign?{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
            if (res == null)
            {
                return new List<Campaign>();
            }
            var campaigns = JsonConvert.DeserializeObject<List<Campaign>>(JsonConvert.SerializeObject(res));
            foreach (Campaign campaign in campaigns)
            {
                campaign.CreatedDate = campaign.CreatedDate.GetTimeWithOffset(timezone);
                campaign.ModifiedDate = campaign.ModifiedDate.HasValue 
                    ? campaign.ModifiedDate.Value.GetTimeWithOffset(timezone)
                    : null;
                campaign.SendDate = campaign.SendDate.Value.GetTimeWithOffset(timezone);
            }
            return campaigns;
        }

        public async Task<ResponseModel> GetCampaignById(int id)
        {
            try
            {
                var param = $"id={id}";
                var res = await APIHelper.SearchTemplateAsync($"/api/CampaignAPI/GetCampaignById?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (res == null)
                {
                    throw new Exception("Campaign not found");
                }
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Data = JsonConvert.DeserializeObject<Campaign>(JsonConvert.SerializeObject(res))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when trying to get Campaign Id = {id}: {ex.Message}");
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel> UpdateCampaign(CampaignDTO model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.SenderName))
                {
                    return new ResponseModel { IsSuccessful = false, Message = "Required fields cannot be left blank" };
                }

                if (model.SendType == (int)ESendType.Scheduled && model.SendDate.Value.Subtract(DateTime.Now).TotalMinutes < 30)
                {
                    return new ResponseModel { IsSuccessful = false, Message = "Scheduled date must be at least 30 minutes later from now" };
                }

                var getWebsiteParam = $"id={model.WebsiteGuid}";
                var findWeb = await APIHelper.SearchTemplateAsync($"/api/WebsiteAPI/GetWebsiteById?{getWebsiteParam}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (findWeb == null)
                {
                    throw new Exception("Current account not found");
                }
                var webId = JsonConvert.DeserializeObject<Website>(JsonConvert.SerializeObject(findWeb)).Id;
                model.WebsiteId = webId;

                model.SendDate = model.SendDate.Value.GetUTCTime(model.Timezone);
                model.SendStatus = (int)ESendStatus.Waiting;
                var campaign = new Campaign();
                var props = typeof(Campaign).GetProperties();
                foreach (var prop in props)
                {
                    if (prop.Name == "SendStatusAtView" || prop.Name == "SendTypeAtView" || prop.Name == "ChannelAtView")
                    {
                        break;
                    }
                    prop.SetValue(campaign, prop.GetValue(model));
                }

                var res = await APIHelper.PostTemplateAsync(campaign, $"/api/CampaignAPI/UpdateCampaign", _apiUrl, _superHeaderName, _superHeaderValue);
                var response = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(res));
                if (!response.IsSuccessful)
                {
                    throw new Exception(response.Message);
                }
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when trying to update Campaign Id = {model.Id}, WebsiteId = {model.WebsiteId}: {ex.Message}");
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = "Some errors occurred"
                };
            }

        }
    }
}
