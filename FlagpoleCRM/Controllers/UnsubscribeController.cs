using Common.Helper;
using Common.Models;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlagpoleCRM.Controllers
{
    public class UnsubscribeController : Controller
    {
        private readonly ILogger<UnsubscribeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;
        public UnsubscribeController(ILogger<UnsubscribeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiUrl = _configuration["APIUrl"];
            _superHeaderName = _configuration["SuperHeader:Name"];
            _superHeaderValue = _configuration["SuperHeader:Value"];
        }
        public async Task<IActionResult> Index(string token)
        {
            try
            {
                var validateJwt = JwtHelper.ValidateToken(token, _configuration);
                if (!validateJwt.IsSuccessful)
                {
                    return Json("Unsubscribe failed. Please try again later");
                }
                var model = JsonConvert.DeserializeObject<JwtModel>(JsonConvert.SerializeObject(validateJwt.Data));
                var unsub = new UnsubcribedEmail
                {
                    Email = model.Email,
                    WebsiteId = model.WebsiteId
                };
                var resObj = await APIHelper.PostTemplateAsync(unsub, $"/api/UnsubscribeAPI/UpdateUnsubscribe", _apiUrl, _superHeaderName, _superHeaderValue);
                if (resObj == null)
                {
                    throw new Exception($"Update unsub return null at Email = {unsub.Email}");
                }
                var res = JsonConvert.DeserializeObject<ResponseModel>(JsonConvert.SerializeObject(resObj));
                if (!res.IsSuccessful)
                {
                    throw new Exception($"Error at Email = {unsub.Email}: {res.Message}");
                }

            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when unsubscribe email: {ex.Message}");
                return Json("Some errors occurred. Please try again later");
            }
            return Json("Unsubscribe successfully");
        }
    }
}
