using Common.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FlagpoleCRM.Controllers
{
    public class CustomerFieldController : Controller
    {
        private readonly ILogger<CustomerFieldController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;

        public CustomerFieldController(ILogger<CustomerFieldController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiUrl = _configuration["APIUrl"];
            _superHeaderName = _configuration["SuperHeader:Name"];
            _superHeaderValue = _configuration["SuperHeader:Value"];
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<List<CustomerField>> GetCustomerFields()
        {
            try
            {
                var fields = await APIHelper.SearchTemplateAsync($"/api/CustomerFieldAPI/GetCustomerFields", _apiUrl, _superHeaderName, _superHeaderValue);
                return JsonConvert.DeserializeObject<List<CustomerField>>(JsonConvert.SerializeObject(fields));
            }
            catch(Exception e)
            {
                _logger.LogError($"Get customer fields failed: {e.Message}");
                return new List<CustomerField>();
            }   
        }

        public async Task<string> GetDataTypeByName(string fieldName)
        {
            var res = await APIHelper.SearchTemplateAsync($"/api/CustomerFieldAPI/GetDataTypeByName?fieldName={fieldName}", _apiUrl, _superHeaderName, _superHeaderValue);
            if (res != null) return res.ToString();
            else
            {
                _logger.LogError($"Field name {fieldName} not found");
                return null;
            }
        }
    }
}
