using Common.Helper;
using Common.Models;
using DataServiceLib;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;

namespace SyncAudiences
{
    public class SyncAudienceWorker : BackgroundService
    {
        private readonly ILogger<SyncAudienceWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public SyncAudienceWorker(ILogger<SyncAudienceWorker> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new Stopwatch();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start sync audiences");
                timer.Start();
                var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                var client = new RestClient(options);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                    var websiteService = scope.ServiceProvider.GetRequiredService<IWebsiteService>();
                    var websites = websiteService.GetAllWebsites().Where(x => !x.IsDeleted).ToList();

                    foreach(Website website in websites)
                    {
                        var audiences = customerService.GetAudiences(website.Guid).Where(x => x.IsHasModification).ToList();
                        foreach(Audience audience in audiences)
                        {
                            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Process audience {audience.Id} of website {website.Guid}");
                            SyncProcess(audience, client, customerService);
                            audience.IsHasModification = false;
                            var audienceDTO = new AudienceDTO(audience);
                            var updateAudienceResponse = customerService.InsertAudience(audienceDTO);
                            if (!updateAudienceResponse.IsSuccessful)
                            {
                                _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error when update audience {audience.Id}: {updateAudienceResponse.Message}");
                            }
                        }
                    };
                }
                timer.Stop();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Sync audience done in {timer.Elapsed.ToString(@"hh\:mm\:ss\.ffff")}");
                timer.Reset();
                await Task.Delay(30000, stoppingToken);
            }
            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Stop sync audience");
        }

        private void SyncProcess(Audience audience, RestClient client, ICustomerService customerService)
        {
            var availableItems = customerService.GetAudienceCustomersByAudienceId(audience.Id);
            foreach(AudienceCustomer item in availableItems)
            {
                item.IsDeleted = true;
                var resp = customerService.UpdateAudienceCustomer(item);
                if (!resp.IsSuccessful)
                {
                    _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Delete AudienceCustomer failed at Id = {item.Id}: {resp.Message}");
                }
            }

            var elasticQuery = JsonConvert.DeserializeObject<ElasticQuery>(audience.ElasticQuery);
            elasticQuery.Source = true;
            elasticQuery.Size = (audience.Limit > 0 && audience.Limit < 10000) ? audience.Limit.Value : 10000;
            var searchAfterQuery = new ElasticSearchAfter(elasticQuery);
            object rs = SearchAllMatchCustomers(searchAfterQuery, client);
            var error = (string)rs.GetType().GetProperty("Error").GetValue(rs, null);
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError($"Get all customers failed: WebsiteId: {audience.WebsiteId}, AudienceID: {audience.Id}, Error: {error}");
                return;
            }
            var customerIds = (List<string>)rs.GetType().GetProperty("customerIdsResult").GetValue(rs, null);
            foreach (string customerId in customerIds)
            {
                var existAudienceCustomer = customerService.FindAudienceCustomer(audience.Id, customerId);
                if (existAudienceCustomer == null)
                {
                    var insertResponse = customerService.InsertAudienceCustomer(audience.Id, customerId);
                    if (!insertResponse.IsSuccessful)
                    {
                        _logger.LogError($"Insert audience-customer failed: WebsiteId: {audience.WebsiteId}, AudienceID: {audience.Id}, CustomerId: {customerId}, Error: {error}");
                        continue;
                    }
                }
                else
                {
                    if (existAudienceCustomer.IsDeleted)
                    {
                        existAudienceCustomer.IsDeleted = false;
                        var resp = customerService.UpdateAudienceCustomer(existAudienceCustomer);
                        if (!resp.IsSuccessful)
                        {
                            _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Recover AudienceCustomer failed at Id = {existAudienceCustomer.Id}: {resp.Message}");
                        }
                    }
                }
            }
        }

        private object SearchAllMatchCustomers(ElasticSearchAfter elasticQuery, RestClient client)
        {
            var customerIdsResult = new List<string>();
            long lastShardDoc = -1;

            var leadCounted = ElasticHelper.CountLeads(elasticQuery, "flagpolecrm.customer", client);
            var totalCount = 0;
            if (elasticQuery.Size > 0 && elasticQuery.Size <= leadCounted)
            {
                totalCount = elasticQuery.Size;
            }
            else
            {
                totalCount = leadCounted;
            }

            string deletePit = "";
            while (customerIdsResult.Count < totalCount)
            {
                var elasticSearchResult = ElasticHelper.SearchAfterElastic(ref elasticQuery, "flagpolecrm.customer", client, ref lastShardDoc, lastShardDoc == -1);
                if (elasticQuery.Pit == null)
                {
                    _logger.LogInformation("Pit is null");
                }
                if (!string.IsNullOrWhiteSpace(elasticSearchResult.Error))
                {
                    _logger.LogError("Error while searching for all customers: " + elasticSearchResult.Error);
                    deletePit = ElasticHelper.DeletePit(client, elasticQuery.Pit.Id);
                    if (!string.IsNullOrEmpty(deletePit))
                    {
                        _logger.LogError($"Error while deleting PIT of PitId = {elasticQuery.Pit.Id}");
                    }
                    return new { Error = elasticSearchResult.Error };
                }
                var customerIds = elasticSearchResult.Hits.Hits.Select(s => s.Id).ToList();

                foreach (string id in customerIds)
                {
                    customerIdsResult.Add(id);
                }
            }

            // Delete pit
            deletePit = elasticQuery.Pit != null ? ElasticHelper.DeletePit(client, elasticQuery.Pit.Id) : "";
            if (!string.IsNullOrEmpty(deletePit))
            {
                _logger.LogError($"Error while deleting PIT of PitId = {elasticQuery.Pit.Id}");
            }

            return new { Error = "", customerIdsResult };
        }
    }
}