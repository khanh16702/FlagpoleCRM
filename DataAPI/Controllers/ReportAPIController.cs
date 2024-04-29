using Common.Constant;
using Common.Enums;
using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDb;
        private IServer _server;
        public ReportAPIController(IConfiguration configuration, ILog log, IConnectionMultiplexer connectionMultiplexer)
        {
            _configuration = configuration;
            _log = log;
            _connectionMultiplexer = connectionMultiplexer;
            _redisDb = _connectionMultiplexer.GetDatabase(0);
            _server = _connectionMultiplexer.GetServer(_configuration["RedisConnection:Host"] + ":" + _configuration["RedisConnection:Port"]);
        }

        [Route("GetTotalOrders")]
        [HttpGet]
        public ResponseModel GetTotalOrders(string websiteId)
        {
            try
            {
                int value = 0;
                var totalOrders = _redisDb.StringGet(RedisKeyPrefix.REPORT_TOTAL_ORDERS + websiteId + ":All");
                if (totalOrders != RedisValue.Null)
                {
                    value = int.Parse(totalOrders);
                }
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Data = value
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        [Route("GetTotalRevenue")]
        [HttpGet]
        public ResponseModel GetTotalRevenue(string websiteId)
        {
            try
            {
                double value = 0.0;
                var totalRevenue = _redisDb.StringGet(RedisKeyPrefix.REPORT_TOTAL_REVENUE + websiteId + ":All");
                if (totalRevenue != RedisValue.Null)
                {
                    value = double.Parse(totalRevenue);
                }
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Data = value
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        [Route("GetRFMScore")]
        [HttpGet]
        public ResponseModel GetRFMScore(string websiteId)
        {
            try
            {
                var rfmScores = new List<int>();
                var rfmKeyPrefix = RedisKeyPrefix.REPORT_RFM + websiteId + ":";
                var rfms = Enum.GetValues(typeof(ERFM));
                foreach (var rfm in rfms)
                {
                    var key = rfmKeyPrefix + rfm;
                    rfmScores.Add(int.Parse(_redisDb.StringGet(key)));
                }
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Data = rfmScores
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        [Route("GetRevenue")]
        [HttpGet]
        public ResponseModel GetRevenue(string websiteId)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var data = new List<List<double>>();

                var arr = new List<HighchartModel>();
                var keys = _server.Keys(pattern: RedisKeyPrefix.REPORT_TOTAL_REVENUE + websiteId + ":*");
                foreach (var key in keys)
                {
                    var name = key.ToString();
                    var nameParts = name.Split(":");
                    if (nameParts[nameParts.Length - 1] == "All")
                    {
                        continue;
                    }
                    var datetime = DateTime.ParseExact(nameParts[nameParts.Length - 1], "dd/MM/yyyy", null);
                    var timestamp = ((DateTimeOffset)datetime).ToUnixTimeMilliseconds();
                    var value = _redisDb.StringGet(key);
                    var model = new HighchartModel
                    {
                        Timestamp = timestamp,
                        Value = double.Parse(value)
                    };
                    arr.Add(model);
                }
                arr = arr.OrderBy(x => x.Timestamp).ToList();
                foreach(var item in arr)
                {
                    var datumn = new List<double> { item.Timestamp.Value, item.Value.Value };
                    data.Add(datumn);
                }
                response.Data = data;
                return response;
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [Route("GetEmailsSent")]
        [HttpGet]
        public ResponseModel GetEmailsSent(string websiteId)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var data = new List<List<double>>();

                var arr = new List<HighchartModel>();
                var keys = _server.Keys(pattern: RedisKeyPrefix.CAMPAIGN_EMAIL_EMAILSSENT + websiteId + ":*");
                foreach (var key in keys)
                {
                    var name = key.ToString();
                    var nameParts = name.Split(":");
                    if (nameParts[nameParts.Length - 1] == "All")
                    {
                        continue;
                    }
                    var datetime = DateTime.ParseExact(nameParts[nameParts.Length - 1], "dd/MM/yyyy", null);
                    var timestamp = ((DateTimeOffset)datetime).ToUnixTimeMilliseconds();
                    var value = _redisDb.StringGet(key);
                    var model = new HighchartModel
                    {
                        Timestamp = timestamp,
                        Value = double.Parse(value)
                    };
                    arr.Add(model);
                }
                arr = arr.OrderBy(x => x.Timestamp).ToList();
                foreach (var item in arr)
                {
                    var datumn = new List<double> { item.Timestamp.Value, item.Value.Value };
                    data.Add(datumn);
                }
                response.Data = data;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
