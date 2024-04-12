using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public class APIHelper
    {
        public static async Task<Object> PostTemplateAsync(Object obj, string link, string apiUrlBase, 
            string superHeaderName, string superHeaderValue)
        {
            HttpClient client = new HttpClient();

            try
            {
                client.BaseAddress = new Uri(apiUrlBase);
                client.Timeout = TimeSpan.FromMinutes(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add(superHeaderName, superHeaderValue);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            ResponseModel cResult = new ResponseModel();
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(link, obj);
                var error = response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    cResult = await response.Content.ReadAsAsync<ResponseModel>();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return cResult;
        }

        public static async Task<Object> SearchTemplateAsync(string link, string apiUrlBase,
            string superHeaderName, string superHeaderValue)
        {
            HttpClient client = new HttpClient();

            try
            {
                client.BaseAddress = new Uri(apiUrlBase);
                client.Timeout = TimeSpan.FromMinutes(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add(superHeaderName, superHeaderValue);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            Object obj = new Object();
            try
            {
                HttpResponseMessage response = await client.GetAsync(link);
                var error = response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    obj = await response.Content.ReadAsAsync<Object>();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return obj;
        }

        public static async Task<ResponseModel> DeleteTemplateAsync(string link, string apiUrlBase,
            string superHeaderName, string superHeaderValue)
        {
            HttpClient client = new HttpClient();

            try
            {
                client.BaseAddress = new Uri(apiUrlBase);
                client.Timeout = TimeSpan.FromMinutes(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add(superHeaderName, superHeaderValue);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            ResponseModel cResult = new ResponseModel();

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(link);
                if (response.IsSuccessStatusCode)
                {
                    cResult = await response.Content.ReadAsAsync<ResponseModel>();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return cResult;
        }
    }
}
