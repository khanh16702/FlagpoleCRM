using Common.Helper;
using Common.Models;
using DataServiceLib;
using MongoDB.Driver;
using RestSharp.Authenticators;
using RestSharp;
using System.Diagnostics;
using PushCustomers.Helper;

namespace PushCustomers
{
    public class PushCustomersWorker : BackgroundService
    {
        private readonly ILogger<PushCustomersWorker> _logger;
        private readonly IConfiguration _configuration;

        public PushCustomersWorker(ILogger<PushCustomersWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _mongoCustomerRaw = new MongoDbHelper<CustomerRaw>(_configuration["MongoDbConnection"], "FlagpoleCRM", "CustomerQueue");
            var _mongoCustomer = new MongoDbHelper<Customer>(_configuration["MongoDbConnection"], "FlagpoleCRM", "Customer");
            var timer = new Stopwatch();

            while (!stoppingToken.IsCancellationRequested)
            {
                timer.Start();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Start push customers");
                var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                var client = new RestClient(options);

                if (_mongoCustomerRaw.CountDocumentsAsync().Result > 0)
                {
                    var customers = _mongoCustomerRaw.Find(x => x.IsSyncCustomer == false).Result.ToList();
                    foreach (CustomerRaw customer in customers)
                    {
                        _logger.LogInformation($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Push customer {customer.Id}");
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
                                _logger.LogError($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Error while pushing customer {customer.Id}: {response.Message}");
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
                                _logger.LogError($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Error while pushing customer {customer.Id}: {response.Message}");
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
                        _logger.LogInformation($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Start calculate RFM");
                        RFMHelper.CalculateRFM(_mongoCustomer, _logger, client);
                    }
                }

                timer.Stop();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Finish push customers in {timer.Elapsed.ToString(@"hh\:mm\:ss\.ffff")}");
                await Task.Delay(30000, stoppingToken);
            }
            _logger.LogInformation($"{DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss")}: Stop push customers");
        }

    }
}
