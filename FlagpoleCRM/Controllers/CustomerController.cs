using Castle.DynamicLinqQueryBuilder;
using Common.Constant;
using Common.Extension;
using Common.Helper;
using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Helper;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Linq.Expressions;

namespace FlagpoleCRM.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;
        private readonly string _superHeaderName;
        private readonly string _superHeaderValue;
        private int? _totalLeads = null;
        public CustomerController(ILogger<CustomerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiUrl = _configuration["APIUrl"];
            _superHeaderName = _configuration["SuperHeader:Name"];
            _superHeaderValue = _configuration["SuperHeader:Value"];
        }
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Request.Cookies.ContainsKey(CookiesName.JWT_COOKIE))
            {
                var token = HttpContext.Request.Cookies[CookiesName.JWT_COOKIE];
                var validateJwt = JwtHelper.ValidateToken(token, _configuration);
                if (!validateJwt.IsSuccessful)
                {
                    return Redirect("/login/authentication/index");
                }

                var userInfo = JsonConvert.DeserializeObject<JwtModel>(JsonConvert.SerializeObject(validateJwt.Data));
                TempData["email"] = userInfo.Email;
                TempData["id"] = userInfo.Id;

                var param = $"email={userInfo.Email}";
                var findAcc = await APIHelper.SearchTemplateAsync($"/api/AccountAPI/GetAccountByEmail?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                var currentAcc = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(findAcc));
                TempData["avatar"] = currentAcc.Avatar;
                TempData["fullName"] = currentAcc.FullName;

                var prm = $"accountId={userInfo.Id}";
                var websitesObj = await APIHelper.SearchTemplateAsync($"/api/WebsiteAPI/GetListWebsites?{prm}", _apiUrl, _superHeaderName, _superHeaderValue);
                var websites = JsonConvert.DeserializeObject<List<Website>>(JsonConvert.SerializeObject(websitesObj));
                if (websites.Count == 0)
                {
                    return Redirect("/website/index");
                }
                ViewBag.WebsiteCount = websites.Count;
                return View();
            }
            else
            {
                return Redirect("/login/authentication/index");
            }
        }

        private int GetTotalLeads(string websiteId, RestClient client)
        {
            try
            {
                var buildedQuery = ElasticHelper.BuildQuery("", 0, "flagpolecrm.customer", websiteId.Replace("-", ""), null, client);
                var elasticQuery = ElasticHelper.GetElasticQuery(buildedQuery);
                return ElasticHelper.CountLeads(elasticQuery, "flagpolecrm.customer", client);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Get total leads error in websiteId = {websiteId}: {ex.Message}");
                return -1;
            }
        }

        public async Task<ActionResult> GetCustomers(SearchCustomerModel model)
        {
            var lst = new List<Hit2>();
            var sort = new List<Dictionary<string, object>>();
            var totalMatchingLeads = 0;
            var ratio = "";
            var strElasticQuery = "";
            var sqlQuery = "";
            var strRulesQueryBuilder = "";

            try
            {
                var param = $"email={model.Email}";
                var findAcc = await APIHelper.SearchTemplateAsync($"/api/AccountAPI/GetAccountByEmail?{param}", _apiUrl, _superHeaderName, _superHeaderValue);
                if (findAcc != null)
                {
                    var indexName = "flagpolecrm.customer";
                    var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
                    options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
                    var client = new RestClient(options);

                    if (_totalLeads == null)
                    {
                        _totalLeads = GetTotalLeads(model.WebsiteId.Replace("-", ""), client);
                    }

                    var currentAcc = JsonConvert.DeserializeObject<Account>(JsonConvert.SerializeObject(findAcc));
                    var timezone = currentAcc.Timezone;

                    var sortItem = new Dictionary<string, object>();
                    sortItem.Add(model.orderColumnName, model.order[0].dir);
                    sort.Add(sortItem);

                    var buildedQuery = ElasticHelper.BuildQuery(model.Condition, model.Limit, indexName, model.WebsiteId.Replace("-", ""), timezone, client);
                    var error = buildedQuery.GetType().GetProperty("Error");
                    if (error == null)
                    {
                        if (!string.IsNullOrEmpty(model.Rules))
                        {
                            var rulesObj = JObject.Parse(model.Rules);
                            if (model.Limit > 0)
                            {
                                rulesObj.Add("limit", model.Limit);
                            }
                            strRulesQueryBuilder = JsonConvert.SerializeObject(rulesObj);
                        }
                        var elasticQuery = ElasticHelper.GetElasticQuery(buildedQuery);
                        strElasticQuery = JsonConvert.SerializeObject(elasticQuery);
                        sqlQuery = ElasticHelper.GetSqlQuery(buildedQuery);

                        totalMatchingLeads = model.Limit > 0 
                            ? Math.Min(ElasticHelper.CountLeads(elasticQuery, "flagpolecrm.customer", client), model.Limit)
                            : ElasticHelper.CountLeads(elasticQuery, "flagpolecrm.customer", client);
                        if (_totalLeads == 0 || totalMatchingLeads == 0)
                        {
                            ratio = "0.00%";
                        }
                        else
                        {
                            ratio = Math.Round(totalMatchingLeads / (double)_totalLeads * 100, 2) + "%";
                        }

                        elasticQuery.Source = true;
                        elasticQuery.Size = (model.Limit > 0 && model.Limit < model.length) ? model.Limit : model.length;
                        elasticQuery.From = model.start;
                        elasticQuery.Sort = sort;
                        var searchResult = ElasticHelper.SearchByQuery(elasticQuery, indexName, client);
                        if (!string.IsNullOrEmpty(searchResult.Error))
                        {
                            throw new Exception("Error in search: " + searchResult.Error);
                        }

                        var pageIndex = model.start / model.length + 1;
                        var limitUsed = model.Limit;
                        var customers = searchResult.Hits.Hits.ToList();
                        var cnt = 0;
                        if (customers.Any())
                        {
                            foreach (Hit2 customer in customers)
                            {
                                cnt++;
                                var itemIndex = cnt + (pageIndex - 1) * model.length;
                                if (limitUsed != 0 && pageIndex * model.length > limitUsed && itemIndex > limitUsed)
                                {
                                    break;
                                }

                                var respItem = new Hit2
                                {
                                    Index = itemIndex,
                                    Id = customer.Id,
                                    Source = customer.Source,
                                    Sort = customer.Sort,
                                    Error = customer.Error
                                };
                                respItem.Source.ChannelSubscribes = respItem.Source.ChannelSubscribes ?? new List<int>();
                                respItem.Source.Tags = respItem.Source.Tags ?? new List<string>();
                                respItem.Source.OrgCreatedDate = respItem.Source.OrgCreatedDate.GetTimeWithOffset(timezone);
                                lst.Add(respItem);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Error while trying to translate SQL query: {(string)error.GetValue(buildedQuery, null)}");
                    }
                }
                else
                {
                    throw new Exception("Cannot find current account");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Get customer list error: " + ex.Message);
            }
            return Json(new 
            { 
                Data = lst, 
                recordsTotal = _totalLeads, 
                recordsFiltered = totalMatchingLeads, 
                ratio, 
                strElasticQuery,
                strRulesQueryBuilder,
                sqlQuery 
            });
        }

        public ActionResult GetCustomerDetail(string id)
        {
            var indexName = "flagpolecrm.customer";
            var options = new RestClientOptions(_configuration["Elasticsearch:Connection"]);
            options.Authenticator = new HttpBasicAuthenticator(_configuration["Elasticsearch:Username"], _configuration["Elasticsearch:Password"]);
            var client = new RestClient(options);

            try
            {
                var customer = ElasticHelper.SearchById(id, client, indexName);
                if (customer == null)
                {
                    throw new Exception("Customer not found");
                }
                return Json(new { data = customer.Source.Contacts });
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error while trying to get customer detail of id = {id}: {ex.Message}");
                return Json(new { data = new List<Contact>() });
            }
        }

        public async Task<List<Audience>> GetAudiences(string websiteId)
        {
            try
            {
                var obj = await APIHelper.SearchTemplateAsync($"/api/CustomerAPI/GetAudiences?websiteId={websiteId}", _apiUrl, _superHeaderName, _superHeaderValue);
                var lst = JsonConvert.DeserializeObject<List<Audience>>(JsonConvert.SerializeObject(obj));
                return lst;
            }
            catch(Exception e)
            {
                _logger.LogError($"Get audiences failed in websiteId = {websiteId}: {e.Message}");
                return new List<Audience>();
            }
        }

        public async Task<ResponseModel> AddAudience(AudienceDTO model)
        {
            var response = new ResponseModel();
            if (string.IsNullOrWhiteSpace(model.Name) || model.Type < 0 || model.Limit < 0)
            {
                _logger.LogError("Add audience: Audience not valid");
                response.IsSuccessful = false;
                response.Message = "Audience not valid";
                return response;
            }

            var prms = $"?name={model.Name}&websiteId={model.WebsiteId}";
            var findAudience = await APIHelper.SearchTemplateAsync($"/api/CustomerAPI/GetAudienceByName{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
            if (findAudience != null)
            {
                var audience = JsonConvert.DeserializeObject<Audience>(JsonConvert.SerializeObject(findAudience));
                if (audience.Id != model.Id)
                {
                    response.IsSuccessful = false;
                    response.Message = "Audience name exists";
                    return response;
                }
            }
            model.IsHasModification = true;
            response = (ResponseModel)await APIHelper.PostTemplateAsync(model, "/api/CustomerAPI/InsertAudience", _apiUrl, _superHeaderName, _superHeaderValue);
            if (!response.IsSuccessful)
            {
                _logger.LogError($"Add audience failed: {response.Message}");
            }
            return response;
        }

        public async Task<ResponseModel> GetAudienceByName(string name, string websiteId)
        {
            var prms = $"?name={name}&websiteId={websiteId}";
            var res = await APIHelper.SearchTemplateAsync($"/api/CustomerAPI/GetAudienceByName{prms}", _apiUrl, _superHeaderName, _superHeaderValue);
            if (res == null)
            {
                _logger.LogError($"Cannot find specified audience name {name} and websiteId {websiteId}");
                return new ResponseModel
                {
                    IsSuccessful = false,
                    Message = "Cannot find specified audience"
                };
            }
            return new ResponseModel
            {
                IsSuccessful = true,
                Data = JsonConvert.DeserializeObject<Audience>(JsonConvert.SerializeObject(res))
            };
        }

        public async Task<ResponseModel> DeleteAudience(string id)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                _ = await APIHelper.SearchTemplateAsync($"/api/CustomerAPI/DeleteAudience?id={id}", _apiUrl, _superHeaderName, _superHeaderValue);
                return response;
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = "Some errors occurred";
                _logger.LogError($"Delete Audience id {id} failed: {ex.Message}");
                return response;
            }
        }

        public ActionResult LoadSaveAudiencePopup(Audience audience)
        {
            return PartialView("/Views/Shared/Customer/_CustomerSaveAudience.cshtml", audience);
        }
    }
}
