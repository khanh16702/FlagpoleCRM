using Common.Constant;
using Common.Helper;
using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using Newtonsoft.Json;
using RestSharp;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net.WebSockets;
using Order = Common.Models.Order;

namespace SyncData
{
    public class SyncDataWorker : BackgroundService
    {
        private readonly ILogger<SyncDataWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDb;

        public SyncDataWorker(ILogger<SyncDataWorker> logger, 
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
            var timer = new Stopwatch();
            var _mongoCustomerRaw = new MongoDbHelper<CustomerRaw>(_configuration["MongoDbConnection"], "FlagpoleCRM", "CustomerQueue");
            var _mongoOrder = new MongoDbHelper<Order>(_configuration["MongoDbConnection"], "FlagpoleCRM", "Order");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Start sync data from ecommerce");
                timer.Start();
                using (var scope = _scopeFactory.CreateScope())
                {
                    var websiteService = scope.ServiceProvider.GetRequiredService<IWebsiteService>();
                    var websites = websiteService.GetAllWebsites().Where(x => !x.IsDeleted).ToList();

                    Parallel.ForEach(websites, new ParallelOptions { MaxDegreeOfParallelism = 3 }, website =>
                    {
                        _redisDb.StringSet(RedisKeyPrefix.REPORT_TOTAL_ORDERS + website.Guid + ":All", 0);
                        _redisDb.StringSet(RedisKeyPrefix.REPORT_TOTAL_REVENUE + website.Guid + ":All", 0);
                        SyncData(website, "orders", _mongoOrder, _mongoCustomerRaw);
                        SyncData(website, "customers", _mongoOrder, _mongoCustomerRaw);
                    });

                    // For debug
                    //foreach (var website in websites)
                    //{
                    //    _redisDb.StringSet(RedisKeyPrefix.REPORT_TOTAL_ORDERS + website.Guid + ":All", 0);
                    //    _redisDb.StringSet(RedisKeyPrefix.REPORT_TOTAL_REVENUE + website.Guid + ":All", 0);
                    //    SyncData(website, "orders", _mongoOrder, _mongoCustomerRaw);
                    //    SyncData(website, "customers", _mongoOrder, _mongoCustomerRaw);
                    //}
                }
                timer.Stop();
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Sync data done in {timer.Elapsed.ToString(@"hh\:mm\:ss\.ffff")}");
                timer.Reset();
                await Task.Delay(21600000, stoppingToken);
            }
            _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Stop sync data from ecommerce");
        }

        private void SyncData(Website website, 
            string collection, 
            MongoDbHelper<Order> _mongoOrder, 
            MongoDbHelper<CustomerRaw> _mongoCustomerRaw)
        {
            List<string> orderDates = new List<string>();

            long sinceId = 0;
            bool isStop = false;

            if (!string.IsNullOrEmpty(website.ShopifyStore) && !string.IsNullOrEmpty(website.ShopifyToken))
            {
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Sync data from Shopify");

                while (!isStop)
                {
                    var response = AccessShopify(website, collection, sinceId);
                    if (!response.IsSuccessful)
                    {
                        _logger.LogError($"Website {website.Id}: Error when trying to sync Shopify {collection}: " + response.Content);
                        return;
                    }
                    switch (collection)
                    {
                        case "orders":
                            ProcessOrders(response, _mongoOrder, website, DataSource.SHOPIFY, ref sinceId, ref isStop, ref orderDates);
                            break;
                        case "customers":
                            ProcessCustomers(response, _mongoOrder, _mongoCustomerRaw, website, DataSource.SHOPIFY, ref sinceId, ref isStop);
                            break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(website.HaravanToken))
            {
                // "sinceId" will not be used in Haravan
                // Haravan uses "page" instead

                isStop = false;
                int page = 1;
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Sync data from Haravan");
                while (!isStop)
                {
                    var response = AccessHaravan(website, collection, page);
                    if (!response.IsSuccessful)
                    {
                        _logger.LogError($"Website {website.Id}: Error when trying to sync Haravan {collection}: " + response.Content);
                        return;
                    }
                    switch (collection)
                    {
                        case "orders":
                            ProcessOrders(response, _mongoOrder, website, DataSource.HARAVAN, ref sinceId, ref isStop, ref orderDates);
                            break;
                        case "customers":
                            ProcessCustomers(response, _mongoOrder, _mongoCustomerRaw, website, DataSource.HARAVAN, ref sinceId, ref isStop);
                            break;
                    }
                    page++;
                }
            }
        }

        private void ProcessOrders(RestResponse response, 
            MongoDbHelper<Order> _mongoOrder, 
            Website website, 
            string source,
            ref long sinceId,
            ref bool isStop,
            ref List<string> orderDates)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Process orders from WebsiteId {website.Guid}");
                var orders = JsonConvert.DeserializeObject<OrderResult>(response.Content).Orders;

                var orderKey = RedisKeyPrefix.REPORT_TOTAL_ORDERS + website.Guid + ":All";
                var totalOrders = int.Parse(_redisDb.StringGet(orderKey));
                totalOrders += orders.Count;
                _redisDb.StringSet(orderKey, totalOrders);

                if (!orders.Any())
                {
                    isStop = true;
                    return;
                }

                int index = 0;
                foreach (OrderRaw order in orders)
                {
                    if (order.CancelledAt == null)
                    {
                        var revenueKey = RedisKeyPrefix.REPORT_TOTAL_REVENUE + website.Guid + ":All";
                        var totalRevenue = double.Parse(_redisDb.StringGet(revenueKey));
                        totalRevenue += order.TotalPrice;
                        _redisDb.StringSet(revenueKey, totalRevenue);

                        var createdDate = order.CreatedAt.ToString("dd/MM/yyyy");
                        var orderRevenueKey = RedisKeyPrefix.REPORT_TOTAL_REVENUE + website.Guid + ":" + createdDate;
                        if (!orderDates.Contains(createdDate))
                        {
                            orderDates.Add(createdDate);
                            _redisDb.StringSet(orderRevenueKey, order.TotalPrice);
                        }
                        else
                        {
                            var orderRevenue = double.Parse(_redisDb.StringGet(orderRevenueKey));
                            orderRevenue += order.TotalPrice;
                            _redisDb.StringSet(orderRevenueKey, orderRevenue);
                        }
                    }
                    if (index == orders.Count - 1)
                    {
                        sinceId = order.OrgId;
                    }
                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Process order {order.OrgId} from {source}");
                    var matchOrder = _mongoOrder.Find(x => x.OrgId == order.OrgId
                        && x.OrgSrc == source
                        && x.WebsiteId == website.Guid.Replace("-",""))
                        .Result.FirstOrDefault();
                    if (matchOrder == null)
                    {
                        var syncResult = SyncOrdersService.Insert(order, website.Guid, source, _mongoOrder);
                        _redisDb.StringSet(RedisKeyPrefix.REPORT_RFM_RECALCULATE + website.Guid, "true");
                        if (!syncResult.IsSuccessful)
                        {
                            _logger.LogError($"Website {website.Id}: Error when trying to sync {source} orders: " + syncResult.Message);
                        }
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Website {website.Id}: Error when trying to sync {source} orders: " + ex.Message);
            }
        }

        private void ProcessCustomers(RestResponse response,
            MongoDbHelper<Order> _mongoOrder,
            MongoDbHelper<CustomerRaw> _mongoCustomerRaw,
            Website website, 
            string source,
            ref long sinceId,
            ref bool isStop)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Process customers from WebsiteId {website.Guid}");
                var customers = JsonConvert.DeserializeObject<CustomerResult>(response.Content).Customers;

                if (customers.Count == 0)
                {
                    isStop = true;
                    return;
                }

                int index = 0;
                foreach (CustomerRaw customer in customers)
                {
                    if (index == customers.Count - 1)
                    {
                        sinceId = customer.Id;
                    }
                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Process customer {customer.Id} from {source}");
                    var matchCustomer = _mongoCustomerRaw.Find(x => x.Id == customer.Id
                        && x.WebsiteId == website.Guid.Replace("-","")
                        && x.OrgSrc == source)
                        .Result
                        .FirstOrDefault();

                    var syncResult = new ResponseModel { IsSuccessful = true };
                    if (matchCustomer != null)
                    {
                        if (matchCustomer.Email == customer.Email
                            && matchCustomer.FirstName == customer.FirstName
                            && matchCustomer.LastName == customer.LastName
                            && matchCustomer.TotalOrders == customer.TotalOrders
                            && matchCustomer.TotalSpent == customer.TotalSpent
                            && matchCustomer.LastOrderId == customer.LastOrderId
                            && matchCustomer.Tags == customer.Tags
                            && matchCustomer.Phone == customer.Phone
                            && matchCustomer.Birthday == customer.Birthday)
                        {
                            if (matchCustomer.Addresses == null && customer.Addresses == null) 
                            {
                                index++;
                                continue;
                            }
                            else
                            {
                                if (matchCustomer.Addresses != null && customer.Addresses != null 
                                    && matchCustomer.Addresses.Count == customer.Addresses.Count)
                                {
                                    var isSame = true;
                                    for (int i = 0; i < customer.Addresses.Count; i++)
                                    {
                                        if (matchCustomer.Addresses[i].Id == customer.Addresses[i].Id
                                            && matchCustomer.Addresses[i].CustomerId == customer.Addresses[i].CustomerId
                                            && matchCustomer.Addresses[i].Name == customer.Addresses[i].Name
                                            && matchCustomer.Addresses[i].FirstName == customer.Addresses[i].FirstName
                                            && matchCustomer.Addresses[i].LastName == customer.Addresses[i].LastName
                                            && matchCustomer.Addresses[i].Company == customer.Addresses[i].Company
                                            && matchCustomer.Addresses[i].Address1 == customer.Addresses[i].Address1
                                            && matchCustomer.Addresses[i].Address2 == customer.Addresses[i].Address2
                                            && matchCustomer.Addresses[i].Phone == customer.Addresses[i].Phone)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            isSame = false;
                                            break;
                                        }
                                    }
                                    if (isSame)
                                    {
                                        index++;
                                        continue;
                                    }
                                }
                            }
                        }
                        syncResult = SyncCustomersRawService.Update(customer, matchCustomer.ObjId, _mongoOrder, _mongoCustomerRaw, website.Guid, source);
                    }
                    else
                    {
                        syncResult = SyncCustomersRawService.Insert(customer, website.Guid, source, _mongoCustomerRaw, _mongoOrder);
                    }
                    if (!syncResult.IsSuccessful)
                    {
                        _logger.LogError($"Website {website.Id}: Error when trying to sync {source} customers: " + syncResult.Message);
                    }
                    index++;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Website {website.Id}: Error when trying to sync {source} customers: " + ex.Message);
            }
        }

        private RestResponse AccessShopify(Website website, string collection, long sinceId)
        {
            var client = new RestClient($"https://{website.ShopifyStore}.myshopify.com");
            client.AddDefaultHeader("X-Shopify-Access-Token", website.ShopifyToken);
            client.AddDefaultQueryParameter("limit", "250");
            client.AddDefaultQueryParameter("since_id", sinceId.ToString());

            var request = new RestRequest($"/admin/api/{_configuration.GetSection("Shopify:Version").Value}/{collection}.json", Method.Get);
            if (collection == "orders")
            {
                client.AddDefaultQueryParameter("status", "any");
            }
            return client.Execute(request);
        }

        private RestResponse AccessHaravan(Website website, string collection, int page)
        {
            var client = new RestClient($"https://apis.haravan.com");
            client.AddDefaultHeader("Authorization", "Bearer " + website.HaravanToken);
            client.AddDefaultQueryParameter("limit", "1000");
            client.AddDefaultQueryParameter("page", page.ToString());
            var request = new RestRequest($"/com/{collection}.json", Method.Get);
            return client.Execute(request);
        }
    }
}