using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICampaignService _campaignService;
        private readonly ILog _log;
        public CampaignAPIController(IConfiguration configuration, ICampaignService campaignService, ILog log)
        {
            _configuration = configuration;
            _campaignService = campaignService;
            _log = log;
        }

        [Route("GetEmailSenderById")]
        [HttpGet]
        public EmailAccount GetEmailSenderById(int id)
        {
            return _campaignService.GetEmailSenderById(id);
        }

        [Route("GetListEmailSender")]
        [HttpGet]
        public List<EmailAccount> GetListEmailSender(string websiteId)
        {
            return _campaignService.GetListEmailSender(websiteId);
        }

        [Route("UpdateEmailSender")]
        [HttpPost]
        public ResponseModel UpdateEmailSender(EmailAccount model)
        {
            return _campaignService.UpdateEmailSender(model);
        }

        [Route("GetCampaignById")]
        [HttpGet]
        public Campaign GetCampaignById(int id)
        {
            return _campaignService.GetCampaignById(id);
        }

        [Route("GetListCampaign")]
        [HttpGet]
        public List<Campaign> GetListCampaign(string websiteId)
        {
            return _campaignService.GetListCampaign(websiteId);
        }

        [Route("UpdateCampaign")]
        [HttpPost]
        public ResponseModel UpdateCampaign(Campaign model)
        {
            return _campaignService.UpdateCampaign(model);
        }
    }
}
