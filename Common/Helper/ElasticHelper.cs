using Common.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Globalization;
using System.Text.Json.Nodes;

namespace Common.Helper
{
    public class ElasticHelper
    {
        /// <summary>
        /// Xây dựng native query để truy vấn elasticsearch
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="limit"></param>
        /// <param name="indexName"></param>
        /// <param name="timeZone"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static object BuildQuery(string condition, int limit, string indexName, string websiteId, string? timeZone, RestClient client)
        {
            var fullSqlQuery = limit > 0 ? $"SELECT TOP {limit} " : "SELECT ";
            fullSqlQuery += $"CreatedDate FROM \"{indexName}\" WHERE ";
            if (!string.IsNullOrEmpty(condition))
            {
                fullSqlQuery += $"{condition}";
                fullSqlQuery += " AND ";
            }
            fullSqlQuery += $"(WebsiteId = '{websiteId}')";
            var body = new
            {
                query = fullSqlQuery,
            };
            var request = new RestRequest("/_sql/translate", Method.Post);
            request.AddJsonBody(body);
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                return new { Error = response.Content };
            }
            var convertedQuery = timeZone != null ? response.Content
                .Replace("\"v1\":\"Z\"", $"\"v1\":\"{timeZone}\"")
                .Replace("\"time_zone\":\"Z\"", $"\"time_zone\":\"{timeZone}\"")
                : response.Content;

            return new { ElasticQuery = JsonConvert.DeserializeObject<ElasticQuery>(convertedQuery), SqlQuery = fullSqlQuery };
        } 

        /// <summary>
        /// Search với id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static Hit2 SearchById(string id, RestClient client, string indexName)
        {
            var request = new RestRequest($"/{indexName}/_doc/{id}", Method.Get);
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                return new Hit2 { Error = response.Content };
            }
            else
            {
                return JsonConvert.DeserializeObject<Hit2>(response.Content);
            }

        }

        /// <summary>
        /// Tạo mới index
        /// </summary>
        /// <param name="name"></param>
        /// <param name="client"></param>
        public static ResponseModel CreateIndex(string name, RestClient client)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var request = new RestRequest($"/{name}", Method.Put);
                client.Execute(request);
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Insert bản ghi
        /// </summary>
        /// <param name="document"></param>
        /// <param name="id"></param>
        /// <param name="client"></param>
        public static ResponseModel InsertDocument(object document, string id, string indexName, RestClient client)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var request = new RestRequest($"/{indexName}/_doc/{id}", Method.Put);
                var body = JsonConvert.SerializeObject(document);
                request.AddJsonBody(body);
                var insertResponse = client.Execute(request);
                if (!insertResponse.IsSuccessful)
                {
                    response.IsSuccessful = false;
                    response.Message = insertResponse.Content;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public static ResponseModel UpdateDocument(object document, string id, string indexName, RestClient client)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var request = new RestRequest($"/{indexName}/_update/{id}", Method.Post);
                var body = new
                {
                    doc = document
                };
                request.AddJsonBody(JsonConvert.SerializeObject(body));
                var updateResponse = client.Execute(request);
                if (!updateResponse.IsSuccessful)
                {
                    response.IsSuccessful = false;
                    response.Message = updateResponse.Content;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Tìm kiếm theo truy vấn
        /// </summary>
        public static ElasticSearchResult SearchByQuery(ElasticQuery elasticQuery, string indexName, RestClient client)
        {
            var request = new RestRequest($"/{indexName}/_search", Method.Post);
            request.AddJsonBody(JsonConvert.SerializeObject(elasticQuery));
            var response = client.Execute(request);
            var result = "";

            if (!response.IsSuccessful)
            {
                result = JsonConvert.SerializeObject(new { error = response.Content });
            }
            else
            {
                result = response.Content;
            }
            return JsonConvert.DeserializeObject<ElasticSearchResult>(result);
        }

        /// <summary>
        /// Tìm kiếm bằng search after
        /// </summary>
        public static ElasticSearchResult SearchAfterElastic(
            ref ElasticSearchAfter elasticQuery, 
            RestClient client,
            ref long lastShardDoc, 
            bool isFirstLoad = false)
        {
            var result = "";
            var pit = new Pit();

            if (isFirstLoad)
            {
                // Tạo pit mới
                var createPit = GeneratePit(client, "15m");   // keep_alive = 15 minutes
                var error = createPit.GetType().GetProperty("Error");
                if (error != null)
                {
                    result = JsonConvert.SerializeObject(new { Error = (string)error.GetValue(createPit, null) });
                    return JsonConvert.DeserializeObject<ElasticSearchResult>(result);
                }
                else
                {
                    pit = (Pit)createPit;
                }
            }

            var request = new RestRequest("/_search", Method.Post);
            if (elasticQuery.Pit == null)
            {
                elasticQuery.Pit = pit;
            }
            if (lastShardDoc != -1)
            {
                elasticQuery.SearchAfter = new List<object>() { lastShardDoc };
            }

            var bodyObj = new object();
            if (elasticQuery.SearchAfter == null)
            {
                bodyObj = new
                {
                    size = elasticQuery.Size,
                    query = elasticQuery.Query,
                    sort = elasticQuery.Sort,
                    pit = elasticQuery.Pit,
                    track_total_hits = false
                };
            }
            else
            {
                bodyObj = new
                {
                    size = elasticQuery.Size,
                    query = elasticQuery.Query,
                    sort = elasticQuery.Sort,
                    pit = elasticQuery.Pit,
                    search_after = elasticQuery.SearchAfter,
                    track_total_hits = false
                };
            }

            var jsonBody = JsonConvert.SerializeObject(bodyObj);
            request.AddJsonBody(jsonBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                result = JsonConvert.SerializeObject(new { Error = response.Content });
            }
            else
            {
                result = response.Content;
            }

            var rs = JsonConvert.DeserializeObject<ElasticSearchResult>(result);
            if (rs.Hits.Hits != null && rs.Hits.Hits.Count > 0)
            {
                lastShardDoc = (long)rs.Hits.Hits[rs.Hits.Hits.Count - 1].Sort[0];
            }

            return JsonConvert.DeserializeObject<ElasticSearchResult>(result);
        }

        /// <summary>
        /// Đếm số bản ghi khớp
        /// </summary>
        public static int CountLeads(ElasticQuery elasticQuery, string indexName, RestClient client)
        {
            var countQueryJson = JsonConvert.SerializeObject(
                JsonConvert.DeserializeObject<ElasticCountQuery>(JsonConvert.SerializeObject(elasticQuery)));
            var request = new RestRequest($"/{indexName}/_count", Method.Post);
            request.AddJsonBody(countQueryJson);
            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<ElasticCountResult>(response.Content).Count;

        }

        /// <summary>
        /// Tạo ra 1 PIT (point in time) để dùng cho searchafter
        /// </summary>
        /// <param name="elasticQuery"></param>
        /// <param name="elasticUrl"></param>
        /// <param name="keepAlive"></param>
        /// <returns></returns>
        public static object GeneratePit(RestClient client, string keepAlive)
        {
            var request = new RestRequest($"/cdp.customer/_pit?keep_alive={keepAlive}", Method.Post);
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                return new { Error = response.Content };
            }
            var pit = JsonConvert.DeserializeObject<Pit>(response.Content);
            pit.KeepAlive = keepAlive;
            return pit;
        }

        /// <summary>
        /// Xoá pit sau khi sử dụng
        /// </summary>
        /// <param name="elasticUrl"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string DeletePit(RestClient client, string id)
        {
            var request = new RestRequest($"/_pit", Method.Delete);
            request.AddJsonBody(new { id });
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                return response.Content;
            }
            return "";
        }

        /// <summary>
        /// Lấy elastic query sau khi build query
        /// </summary>
        public static ElasticQuery GetElasticQuery(object buildedElasticQuery)
        {
            return (ElasticQuery)buildedElasticQuery.GetType().GetProperty("ElasticQuery").GetValue(buildedElasticQuery, null);
        }

        /// <summary>
        /// Lấy sql query sau khi build query
        /// </summary>
        public static string GetSqlQuery(object buildedElasticQuery)
        {
            return (string)buildedElasticQuery.GetType().GetProperty("SqlQuery").GetValue(buildedElasticQuery, null);
        }

        /// <summary>
        /// Kiểm tra index có tồn tại không
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticUrl"></param>
        /// <returns></returns>
        public static bool CheckIndexExist(string name, RestClient client)
        {
            var request = new RestRequest($"/_aliases", Method.Get);
            var response = client.Execute(request);

            var indexList = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            return indexList.Keys.Contains(name);
        }

    }
}
