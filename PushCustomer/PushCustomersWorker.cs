using Common.Helper;
using Common.Models;
using DataServiceLib;
using MongoDB.Driver;
using RestSharp.Authenticators;
using RestSharp;
using System.Diagnostics;
using PushCustomers.Helper;
using FlagpoleCRM.Models;
using FlagpoleCRM.DTO;
using StackExchange.Redis;
using Common.Constant;
using Common.Enums;
using EnumsNET;

namespace PushCustomers
{
    public class PushCustomersWorker : BackgroundService
    {
        private readonly ILogger<PushCustomersWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDb;

        public PushCustomersWorker(ILogger<PushCustomersWorker> logger, 
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
            var _mongoCustomerRaw = new MongoDbHelper<CustomerRaw>(_configuration["MongoDbConnection"], "FlagpoleCRM", "CustomerQueue");
            var _mongoCustomer = new MongoDbHelper<Customer>(_configuration["MongoDbConnection"], "FlagpoleCRM", "Customer");
            var timer = new Stopwatch();

            while (!stoppingToken.IsCancellationRequested)
            {
                timer.Start();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start push customers");
                var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                var client = new RestClient(options);

                bool isRescanAudience = false;  // if Customer index of Elasticsearch changed, Audience table in SQLServer must be rescanned
                if (_mongoCustomerRaw.CountDocumentsAsync().Result > 0)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var websiteService = scope.ServiceProvider.GetRequiredService<IWebsiteService>();
                        var allWebsites = websiteService.GetAllWebsites();
                        foreach(Website website in allWebsites)
                        {
                            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Processing customers in WebsiteId = {website.Guid}");
                            var customers = _mongoCustomerRaw.Find(x => x.WebsiteId == website.Guid.Replace("-", "") && !x.IsSyncCustomer).Result.ToList();
                            foreach (CustomerRaw customer in customers)
                            {
                                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Push customer {customer.Id}");
                                var matchedCustomer =
                                    _mongoCustomer.Find(x => x.Contacts.Any(x => (!string.IsNullOrEmpty(x.Email) && x.Email == customer.Email)
                                    || (!string.IsNullOrEmpty(x.Phone) && x.Phone == customer.Phone)
                                    || (customer.Addresses != null && customer.Addresses.Any(y => (!string.IsNullOrEmpty(y.Phone) && y.Phone == x.Phone)))))
                                    .Result.FirstOrDefault();

                                if (matchedCustomer != null)
                                {
                                    var response = SyncCustomerService.UpdateCustomer(matchedCustomer, customer, _mongoCustomer);
                                    if (!response.IsSuccessful)
                                    {
                                        _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error while pushing customer {customer.Id}: {response.Message}");
                                    }
                                    else
                                    {
                                        customer.IsSyncCustomer = true;
                                        _mongoCustomerRaw.Update(customer.ObjId, customer);
                                    }
                                }
                                else
                                {
                                    var response = SyncCustomerService.InsertCustomer(customer, _mongoCustomer);
                                    if (!response.IsSuccessful)
                                    {
                                        _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error while pushing customer {customer.Id}: {response.Message}");
                                    }
                                    else
                                    {
                                        customer.IsSyncCustomer = true;
                                        _mongoCustomerRaw.Update(customer.ObjId, customer);
                                    }
                                }
                            }

                            // Calculate RFM and push to Elasticsearch
                            if (customers.Any())
                            {
                                isRescanAudience = true;
                                ResetRFMRedis(website.Guid);
                                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start calculate RFM");
                                RFMHelper.CalculateRFM(_mongoCustomer, _logger, client, website.Guid, _redisDb);
                            }
                            else
                            {
                                var rfmRecalKey = RedisKeyPrefix.REPORT_RFM_RECALCULATE + website.Guid;
                                if (_redisDb.StringGet(rfmRecalKey).ToString() == "true")
                                {
                                    ResetRFMRedis(website.Guid);
                                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start calculate RFM");
                                    RFMHelper.CalculateRFM(_mongoCustomer, _logger, client, website.Guid, _redisDb);
                                    _redisDb.StringSet(rfmRecalKey, "false");
                                }
                            }
                        }
                    }
                }

                timer.Stop();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Finish push customers in {timer.Elapsed.ToString(@"hh\:mm\:ss\.ffff")}");
                
                if (isRescanAudience)
                {
                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start modify static audiences");
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                        timer.Restart();
                        _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Getting static audiences...");
                        var audiences = customerService.GetDynamicAudiences();
                        Parallel.ForEach(audiences, new ParallelOptions { MaxDegreeOfParallelism = 3 }, audience =>
                        {
                            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start modify audience {audience.Id}");
                            if (!audience.IsHasModification)
                            {
                                audience.IsHasModification = true;
                                var audienceDTO = new AudienceDTO(audience);
                                var updateAudienceResponse = customerService.InsertAudience(audienceDTO);
                                if (!updateAudienceResponse.IsSuccessful)
                                {
                                    _logger.LogError($"Modify audience failed at AudienceID: {audience.Id}: {updateAudienceResponse.Message}");
                                }
                            }
                        });
                        timer.Stop();
                        _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Finish modify static audiences in {timer.Elapsed.ToString(@"hh\:mm\:ss\.ffff")}");
                    }
                }
                timer.Reset();
                await Task.Delay(3000, stoppingToken);
            }
            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Stop push customers");
        }

        private void ResetRFMRedis(string websiteId)
        {
            var rfmKeyPrefix = RedisKeyPrefix.REPORT_RFM + websiteId + ":";
            var rfms = Enum.GetValues(typeof(ERFM));
            foreach(var rfm in rfms)
            {
                var key = rfmKeyPrefix + rfm;
                _redisDb.StringSet(key, 0);
            }
        }

    }
}
