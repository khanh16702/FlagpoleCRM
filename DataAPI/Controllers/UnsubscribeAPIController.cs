using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnsubscribeAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUnsubscribeService _unsubService;
        private readonly ILog _log;
        public UnsubscribeAPIController(IConfiguration configuration, IUnsubscribeService unsubService, ILog log)
        {
            _configuration = configuration;
            _unsubService = unsubService;
            _log = log;
        }

        [Route("UpdateUnsubscribe")]
        [HttpPost]
        public ResponseModel UpdateUnsubscribe(UnsubcribedEmail model)
        {
            return _unsubService.UpdateUnsubscribe(model);
        }
    }
}
