using Common.Constant;
using Common.Helper;
using Common.Models;
using Common.Utilize;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace FlagpoleCRM.Areas.Login.Controllers
{
    [Area("Login")]
    public class AuthenticationController : Controller
    {
        private readonly ILog _log;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;
        public AuthenticationController(ILog log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
            _apiUrl = _configuration["APIUrl"];
            _superHeaderName = _configuration["SuperHeader:Name"];
            _superHeaderValue = _configuration["SuperHeader:Value"];
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetWebUrl()
        {
            return Json(_configuration.GetSection("WebUrl").Value);
        }

        public async Task<ResponseModel> Login(AccountDTO model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password)) {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Login failed"
                };
            }

            if (!EmailService.ValidateEmail(model.Email))
            {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Please provide a valid email"
                };
            }

            var response = await APIHelper.PostTemplateAsync(model, "/api/AuthenticationAPI/Login", _apiUrl, _superHeaderName, _superHeaderValue);
            return (ResponseModel)response;
        }

        public ResponseModel Logout()
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                if (HttpContext.Request.Cookies.ContainsKey(CookiesName.JWT_COOKIE))
                {
                    Response.Cookies.Delete(CookiesName.JWT_COOKIE);
                }
                else
                {
                    throw new Exception("JWT does not exist");
                }
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
                _log.Error($"Log out failed: {ex.Message}");
            }
            return response;
        }

        public async Task<ResponseModel> Register(AccountDTO model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.RetypedPassword))
            {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Register failed"
                };
            }

            if (!EmailService.ValidateEmail(model.Email))
            {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Please provide a valid email"
                };
            }

            if (model.Password != model.RetypedPassword)
            {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Retyped password is not correct"
                };
            }

            var response = await APIHelper.PostTemplateAsync(model, "/api/authenticationapi/register", _apiUrl, _superHeaderName, _superHeaderValue);
            return (ResponseModel)response;

        }

        public async Task<ResponseModel> ConfirmOTP(ConfirmEmailDTO model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Invalid email",
                };
            }
            if (string.IsNullOrEmpty(model.Code))
            {
                return new ResponseModel()
                {
                    IsSuccessful = false,
                    Message = "Please provide verification code"
                };
            }
            var response = await APIHelper.PostTemplateAsync(model, "/api/authenticationapi/ConfirmOTP", _apiUrl, _superHeaderName, _superHeaderValue);
            return (ResponseModel)response;
        }

        public async Task<ResponseModel> SendOTP(string email)
        {
            var param = $"accountEmail={email}";
            var response = await APIHelper.SearchTemplateAsync($"/api/authenticationapi/SendOTP?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
            return (ResponseModel)response;
        }

        public async Task<ResponseModel> CreateToken(string email)
        {
            try
            {
                var param = $"email={email}";
                var accObj = await APIHelper.SearchTemplateAsync($"/api/AccountAPI/GetAccountByEmail?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var account = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(accObj));

                var response = (ResponseModel)await APIHelper.PostTemplateAsync(account, "/api/authenticationapi/CreateToken", _apiUrl, _superHeaderName, _superHeaderValue);
                if (response.IsSuccessful)
                {
                    HttpContext.Response.Cookies.Append(CookiesName.JWT_COOKIE, (string)response.Data, new CookieOptions
                    {
                        MaxAge = TimeSpan.FromDays(1),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }
                return new ResponseModel { IsSuccessful = response.IsSuccessful };
            }
            catch(Exception ex)
            {
                return new ResponseModel { IsSuccessful = false, Message = ex.Message };
            }
        }
    }
}
