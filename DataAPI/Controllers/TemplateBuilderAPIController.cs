using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateBuilderAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITemplateService _templateService;
        private readonly ILog _log;
        public TemplateBuilderAPIController(IConfiguration configuration, ITemplateService templateService, ILog log)
        {
            _configuration = configuration;
            _templateService = templateService;
            _log = log;
        }

        [Route("GetTemplates")]
        [HttpGet]
        public List<Template> GetTemplates(string websiteId)
        {
            return _templateService.GetTemplates(websiteId);
        }

        [Route("GetTemplateById")]
        [HttpGet]
        public Template GetTemplateById(int id)
        {
            return _templateService.GetTemplateById(id);
        }

        [Route("InsertOrUpdate")]
        [HttpPost]
        public ResponseModel InsertOrUpdate(Template template)
        {
            return _templateService.InsertOrUpdate(template);
        }
    }
}
