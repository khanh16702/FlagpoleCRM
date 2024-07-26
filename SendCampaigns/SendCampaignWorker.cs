using Common.Helper;
using Common.Models;
using DataServiceLib;
using RestSharp.Authenticators;
using RestSharp;
using System.Diagnostics;
using FlagpoleCRM.Models;
using Common.Enums;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Common.Utilize;
using Humanizer;
using Common.Constant;

namespace SendCampaigns
{
    public class SendCampaignWorker : BackgroundService
    {
        private readonly ILogger<SendCampaignWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDb;

        public SendCampaignWorker(ILogger<SendCampaignWorker> logger, 
            IConfiguration configuration, 
            IServiceScopeFactory scopeFactory,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _connectionMultiplexer = connectionMultiplexer;
            _redisDb = _connectionMultiplexer.GetDatabase(0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timer = new Stopwatch();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start send campaigns");
                timer.Start();

                var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                var client = new RestClient(options);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var campaignService = scope.ServiceProvider.GetRequiredService<ICampaignService>();
                    var websiteService = scope.ServiceProvider.GetRequiredService<IWebsiteService>();
                    var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
                    var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                    var unsubscribeService = scope.ServiceProvider.GetRequiredService<IUnsubscribeService>();
                    var campaigns = campaignService.GetProcessCampaigns();
                    foreach(Campaign campaign in campaigns)
                    {
                        var audience = customerService.GetAudienceById(campaign.AudienceId);
                        if (audience.IsHasModification)
                        {
                            continue;
                        }

                        _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Processing CampaignId = {campaign.Id}");

                        campaign.SendStatus = (int)ESendStatus.Sending;
                        _ = campaignService.UpdateCampaign(campaign);
                        
                        var websiteGuid = campaign.WebsiteGuid;
                        var sentList = new List<string>();
                        var success = 0;
                        var failure = 0;


                        if (campaign.Channel == (int)EChannelSubscribe.Email)
                        {
                            var sender = campaignService.GetEmailSenderById(campaign.EmailId.Value);
                            var template = templateService.GetTemplateById(campaign.TemplateId.Value);
                            if (template == null)
                            {
                                continue;
                            }
                            var content = template.Content;
                            
                            int take = 10000, skip = 0;
                            while (true)
                            {
                                var customerIds = customerService.GetAudienceCustomersByAudienceId(campaign.AudienceId)
                                    .Skip(skip)
                                    .Take(take)
                                    .Select(x => x.CustomerId)
                                    .ToList();
                                if (!customerIds.Any())
                                {
                                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Process campaign end at CampaignId = {campaign.Id}");
                                    break;
                                }
                                foreach(string customerId in customerIds)
                                {
                                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Processing CustomerId = {customerId}, CampaignId = {campaign.Id}");
                                    var hit = ElasticHelper.SearchById(customerId, client, "flagpolecrm.customer");
                                    if (!string.IsNullOrEmpty(hit.Error))
                                    {
                                        _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error when getting customer with CustomerId = {customerId}, CampaignId = {campaign.Id}: {hit.Error}");
                                        continue;
                                    }
                                    if (hit.Source.ChannelSubscribes == null 
                                        || (hit.Source.ChannelSubscribes != null && !hit.Source.ChannelSubscribes.Contains((int)EChannelSubscribe.Email))) 
                                    {
                                        continue;
                                    }
                                    var contacts = hit.Source.Contacts.Where(x => !string.IsNullOrWhiteSpace(x.Email)).ToList();
                                    if (contacts.Any())
                                    {
                                        foreach(Contact contact in contacts)
                                        {
                                            if (unsubscribeService.GetUnsubByEmail(contact.Email) == null && !sentList.Contains(contact.Email))
                                            {
                                                var hasNameContact = !string.IsNullOrWhiteSpace(contact.FullName) 
                                                    ? contact
                                                    : contacts.FirstOrDefault(x => x.Email == contact.Email && !string.IsNullOrWhiteSpace(x.FullName));
                                                var name = hasNameContact == null ? null : contact.FullName;

                                                var hasPhoneContact = !string.IsNullOrWhiteSpace(contact.Phone)
                                                    ? contact
                                                    : contacts.FirstOrDefault(x => x.Email == contact.Email && !string.IsNullOrWhiteSpace(x.Phone));
                                                var phone = hasPhoneContact == null ? null : contact.Phone;

                                                var hasAddressContact = !string.IsNullOrWhiteSpace(contact.Address)
                                                    ? contact
                                                    : contacts.FirstOrDefault(x => x.Email == contact.Email && !string.IsNullOrWhiteSpace(x.Address));
                                                var address = hasAddressContact == null ? null : contact.Address;

                                                var birthday = hit.Source.Birthday.HasValue ? hit.Source.Birthday.Value : DateTime.MinValue;

                                                // Process MergeTags
                                                var body = content;
                                                var mergeTagsRegex = new Regex(@"{{[^{}]+}}");
                                                MatchCollection mergeTags = mergeTagsRegex.Matches(body);
                                                foreach(Match mergeTag in mergeTags)
                                                {
                                                    var key = mergeTag.Value.Split("|")[0].Replace("{{", "");
                                                    var value = mergeTag.Value.Split("|")[1].Replace("}}", "");
                                                    switch(key)
                                                    {
                                                        case "Contacts.FullName":
                                                            var actualName = name ?? value;
                                                            body = body.Replace(mergeTag.Value, actualName);
                                                            break;
                                                        case "Contacts.Phone":
                                                            var actualPhone = phone ?? value;
                                                            body = body.Replace(mergeTag.Value, actualPhone);
                                                            break;
                                                        case "Contacts.Address":
                                                            var actualAddress = address ?? value;
                                                            body = body.Replace(mergeTag.Value, actualAddress);
                                                            break;
                                                        case "Contacts.Email":
                                                            body = body.Replace(mergeTag.Value, contact.Email);
                                                            break;
                                                        case "Birthday":
                                                            var actualBirthday = DateTime.Compare(birthday, DateTime.MinValue) == 0
                                                                ? value
                                                                : birthday.ToString("dd/MM/yyyy");
                                                            body = body.Replace(mergeTag.Value, actualBirthday);
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }

                                                // Create unsubscribe token
                                                var claims = new List<Claim>()
                                                {
                                                    new Claim(ClaimTypes.NameIdentifier, campaign.WebsiteGuid),
                                                    new Claim(ClaimTypes.Email, contact.Email)
                                                };
                                                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                                                var credits = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                                                var token = new JwtSecurityToken(claims: claims, expires: new DateTime(2035, 1, 1), signingCredentials: credits);
                                                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                                                body += "<div style=\"margin: 30px; color: #2E5274\">" +
                                                    "If you don't want to receive our email campaigns in the future, click <a href=\"" +
                                                    _configuration.GetSection("WebUrl").Value + "/Unsubscribe/Index?token=" + jwt +
                                                    "\">here</a>" +
                                                    " to unsubscribe</div>";

                                                // Send mail
                                                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start send email to Email = {contact.Email} in CampaignId = {campaign.Id}");
                                                var smtpServer = _configuration["EmailSender:SMTPServer"];
                                                var port = int.Parse(_configuration["EmailSender:Port"]);
                                                var emailBuildResult = EmailService.EmailBuilder(sender.Email, contact.Email, template.Subject, body, sender.Password, smtpServer, port);
                                                var request = (EmailDTO)emailBuildResult.Data.GetType().GetProperty("request").GetValue(emailBuildResult.Data, null);
                                                var smtp = (SmtpDTO)emailBuildResult.Data.GetType().GetProperty("smtp").GetValue(emailBuildResult.Data, null);
                                                var response = EmailService.SendEmail(request, smtp);
                                                if (!response.IsSuccessful)
                                                {
                                                    _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error when send email to Email = {contact.Email} in CampaignId = {campaign.Id}: {response.Message}");
                                                    failure++;
                                                }
                                                else
                                                {
                                                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Done send email to Email = {contact.Email} in CampaignId = {campaign.Id}");
                                                    success++;
                                                    sentList.Add(contact.Email);

                                                    //Update to Redis
                                                    var totalKey = RedisKeyPrefix.CAMPAIGN_EMAIL_EMAILSSENT + websiteGuid + ":All";
                                                    var totalSentAll = int.Parse(_redisDb.StringGet(totalKey));
                                                    _redisDb.StringSet(totalKey, ++totalSentAll);

                                                    var todayKey = RedisKeyPrefix.CAMPAIGN_EMAIL_EMAILSSENT + websiteGuid + ":" + DateTime.UtcNow.ToString("dd/MM/yyyy");
                                                    var totalSentToday = _redisDb.StringGet(todayKey);
                                                    if (totalSentToday == RedisValue.Null)
                                                    {
                                                        _redisDb.StringSet(todayKey, 1);
                                                    }
                                                    else
                                                    {
                                                        var currentSentToday = int.Parse(totalSentToday);
                                                        _redisDb.StringSet(todayKey, ++currentSentToday);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                skip += take;
                            }
                        }

                        // Update campaign
                        var campaignSentKey = RedisKeyPrefix.CAMPAIGN_EMAIL_SENT + websiteGuid + ":All";
                        var totalCampaignSent = int.Parse(_redisDb.StringGet(campaignSentKey));
                        _redisDb.StringSet(campaignSentKey, ++totalCampaignSent);

                        campaign.Success = success;
                        campaign.Failure = failure;
                        campaign.SendStatus = (int)ESendStatus.Complete;
                        _ = campaignService.UpdateCampaign(campaign);

                        _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Done process campaign: CampaignId = {campaign.Id}");
                    }
                }
                timer.Stop();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Send campaigns done in {timer.Elapsed.ToString(@"hh\:mm\:ss\.ffff")}");
                timer.Reset();
            }
            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Stop send campaigns");
        }
    }
}