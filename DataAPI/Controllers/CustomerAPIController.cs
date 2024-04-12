using Common.Models;
using DataServiceLib;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICustomerService _customerService;
        private readonly ILog _log;
        public CustomerAPIController(IConfiguration configuration, ICustomerService customerService, ILog log)
        {
            _configuration = configuration;
            _customerService = customerService;
            _log = log;
        }

        [Route("GetAudiences")]
        [HttpGet]
        public List<Audience> GetAudiences(string websiteId)
        {
            return _customerService.GetAudiences(websiteId);
        }

        [Route("InsertAudience")]
        [HttpPost]
        public ResponseModel InsertAudience(AudienceDTO model)
        {
            return _customerService.InsertAudience(model);
        }

        [Route("GetAudienceByName")]
        [HttpGet]
        public Audience GetAudienceByName(string name, string websiteId)
        {
            return _customerService.GetAudienceByName(name, websiteId);
        }

        [Route("DeleteAudience")]
        [HttpGet]
        public ResponseModel DeleteAudience(string id)
        {
            return _customerService.DeleteAudience(id);
        }
    }
}
