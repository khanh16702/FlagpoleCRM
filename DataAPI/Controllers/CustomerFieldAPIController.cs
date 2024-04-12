using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerFieldAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICustomerFieldService _customerFieldService;
        private readonly ILog _log;
        public CustomerFieldAPIController(IConfiguration configuration, ICustomerFieldService customerFieldService, ILog log)
        {
            _configuration = configuration;
            _customerFieldService = customerFieldService;
            _log = log;
        }

        [Route("GetCustomerFields")]
        [HttpGet]
        public List<CustomerField> GetCustomerFields()
        {
            return _customerFieldService.GetCustomerFields();
        }

        [Route("GetDataTypeByName")]
        [HttpGet]
        public string GetDataTypeByName(string fieldName)
        {
            return _customerFieldService.GetDataType(fieldName);
        }
    }
}
