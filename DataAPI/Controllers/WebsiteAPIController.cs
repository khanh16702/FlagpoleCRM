using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsiteAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebsiteService _websiteService;
        public WebsiteAPIController(IConfiguration configuration, IWebsiteService websiteService)
        {
            _configuration = configuration;
            _websiteService = websiteService;
        }

        [HttpGet]
        [Route("GetListWebsites")]
        public List<Website> GetListWebsites(string accountId)
        {
            return _websiteService.GetWebsitesByAccountId(accountId);
        }

        [HttpPost]
        [Route("CreateWebsite")]
        public ResponseModel CreateWebsite(Website website)
        {
            return _websiteService.Insert(website);
        }

        [HttpPost]
        [Route("UpdateWebsite")]
        public ResponseModel UpdateWebsite(Website website)
        {
            return _websiteService.Update(website);
        }

        [HttpGet]
        [Route("GetWebsiteById")]
        public Website GetWebsiteById(string id)
        {
            return _websiteService.GetWebsiteByGuid(id);
        }
    }
}
