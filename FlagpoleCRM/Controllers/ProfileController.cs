using Common.Constant;
using Common.Extension;
using Common.Helper;
using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FlagpoleCRM.Controllers
{
    public class ProfileController : Controller
    {

        private readonly ILogger<ProfileController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;

        public ProfileController(ILogger<ProfileController> logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _apiUrl = _configuration["APIUrl"];
            _superHeaderName = _configuration["SuperHeader:Name"];
            _superHeaderValue = _configuration["SuperHeader:Value"];
            _environment = environment;
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
                var avatarPathSplit = currentAcc.Avatar.Split("/");
                TempData["avatarName"] = avatarPathSplit[avatarPathSplit.Length - 1];
                TempData["fullName"] = currentAcc.FullName;
                TempData["phone"] = currentAcc.PhoneNumber;
                TempData["timeZone"] = currentAcc.Timezone;
                TempData["createdDate"] = currentAcc.CreatedDate.GetTimeWithOffset(currentAcc.Timezone).ToString("dd/MM/yyyy HH:mm");

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

        public IActionResult GetListTimeZones()
        {
            var timezones = new List<object>();

            try
            {
                using (StreamReader r = new StreamReader(_environment.WebRootPath + "/content/timezones.json"))
                {
                    string json = r.ReadToEnd();
                    JArray arr = JArray.Parse(json);
                    foreach (JObject obj in arr)
                    {
                        var text = obj["text"].ToString();
                        var timezone = new
                        {
                            Text = text,
                            Value = text.Split(" ")[0].Replace("(", "").Replace(")", "").Replace("UTC", "").ToString()
                        };
                        timezones.Add(timezone);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError($"Get timezones failed: {e.Message}");
            }
            return Json(timezones);
        }

        public ResponseModel UploadImage(IFormFile file, string websiteId)
        {
            var response = new ResponseModel
            {
                IsSuccessful = true
            };
            string folderUpload = _environment.WebRootPath + "\\img\\temp";
            var res = FileHelper.UploadFile(file, folderUpload, "/img/temp/");
            if (!res.IsSuccessful)
            {
                _logger.LogError($"Error when uploading image in {websiteId}: {res.Message}");
                response.IsSuccessful = false;
                response.Message = "Some errors occurred";
            }
            else
            {
                response.Data = res.Data;
            }
            return response;
        }

        public void DeleteTempImage(string path)
        {
            var fullPath = _environment.WebRootPath + path;
            var response = FileHelper.DeleteFile(fullPath);
            if (!response.IsSuccessful)
            {
                _logger.LogError($"Error when delete image {path}: {response.Message}");
            }
        }

        public ResponseModel AddAvatar(IFormFile file, string accountId)
        {
            var response = new ResponseModel { IsSuccessful = true };
            if (file == null)
            {
                response.Data = new { Path = "" };
                return response;
            }
            try
            {
                string folderUpload = _environment.WebRootPath + "\\img\\user\\avatar";
                var addAvatarResponse = FileHelper.UploadFile(file, folderUpload, "/img/user/avatar/");
                if (!addAvatarResponse.IsSuccessful)
                {
                    throw new Exception(addAvatarResponse.Message);
                }
                else
                {
                    response.Data = addAvatarResponse.Data;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Upload new avatar failed in accountId = {accountId}: {ex.Message}");
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateAccount(AccountDTO account)
        {
            try
            {
                var param = $"id={account.Id}";
                var accObj = await APIHelper.SearchTemplateAsync($"/api/AccountAPI/GetAccountById?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (accObj == null)
                {
                    throw new Exception($"Account not found: accountId = {account.Id}");
                }
                else
                {
                    var foundAcc = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(accObj));
                    foundAcc.FullName = account.UserName;
                    foundAcc.PhoneNumber = account.PhoneNumber;
                    foundAcc.Password = account.Password ?? "";
                    foundAcc.Timezone = account.Timezone;
                    foundAcc.Avatar = account.Avatar;

                    if (!string.IsNullOrEmpty(foundAcc.PhoneNumber))
                    {
                        foundAcc.PhoneNumberConfirmed = true;
                    }
                    var resObj = await APIHelper.PostTemplateAsync(foundAcc, "/api/AccountAPI/UpdateAccount", _apiUrl, _superHeaderName, _superHeaderValue);
                    var res = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(resObj));
                    return res;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while updating account {account.Id}: {ex.Message}");
                return new ResponseModel { IsSuccessful = false, Message = "Some errors occurred" };
            }
        }
    }
}
