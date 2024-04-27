using Common.Constant;
using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Newtonsoft.Json;
using Common.Enums;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICampaignService _campaignService;
        private readonly ILog _log;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDb;
        public CampaignAPIController(IConfiguration configuration, ICampaignService campaignService, ILog log, IConnectionMultiplexer connectionMultiplexer)
        {
            _configuration = configuration;
            _campaignService = campaignService;
            _log = log;
            _connectionMultiplexer = connectionMultiplexer;
            _redisDb = _connectionMultiplexer.GetDatabase(0);
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
            var response = _campaignService.UpdateCampaign(model);
            if (response.IsSuccessful)
            {
                if (model.Id == 0 && model.Channel == (int)EChannelSubscribe.Email)
                {
                    var totalEmailCampCreated = int.Parse(_redisDb.StringGet(RedisKeyPrefix.CAMPAIGN_EMAIL_TOTAL + model.WebsiteGuid + ":All"));
                    totalEmailCampCreated++;
                    _redisDb.StringSet(RedisKeyPrefix.CAMPAIGN_EMAIL_TOTAL + model.WebsiteGuid + ":All", totalEmailCampCreated);
                }
            }
            return response;
        }

        [Route("GetRedisValue")]
        [HttpGet]
        public ResponseModel GetRedisValue(string websiteId, DateTime today)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var campaignCreatedKey = RedisKeyPrefix.CAMPAIGN_EMAIL_TOTAL + websiteId + ":All";
                if (_redisDb.StringGet(campaignCreatedKey) == RedisValue.Null)
                {
                    _redisDb.StringSet(campaignCreatedKey, 0);
                }

                var campaignSentKey = RedisKeyPrefix.CAMPAIGN_EMAIL_SENT + websiteId + ":All";
                if (_redisDb.StringGet(campaignSentKey) == RedisValue.Null)
                {
                    _redisDb.StringSet(campaignSentKey, 0);
                }

                var emailsSentKey = RedisKeyPrefix.CAMPAIGN_EMAIL_EMAILSSENT + websiteId + ":All";
                if (_redisDb.StringGet(emailsSentKey) == RedisValue.Null)
                {
                    _redisDb.StringSet(emailsSentKey, 0);
                }

                var todayEmailsSentKey = RedisKeyPrefix.CAMPAIGN_EMAIL_EMAILSSENT + websiteId + ":" + today.ToString("dd/MM/yyyy");
                if (_redisDb.StringGet(todayEmailsSentKey) == RedisValue.Null)
                {
                    _redisDb.StringSet(todayEmailsSentKey, 0);
                }

                var campaignStatistic = new CampaignStatisticModel
                {
                    TotalCreated = int.Parse(_redisDb.StringGet(campaignCreatedKey)),
                    TotalSent = int.Parse(_redisDb.StringGet(campaignSentKey)),
                    TotalEmailsSent = int.Parse(_redisDb.StringGet(emailsSentKey)),
                    TotalEmailsSentToday = int.Parse(_redisDb.StringGet(todayEmailsSentKey))
                };
                response.Data = JsonConvert.SerializeObject(campaignStatistic);
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
