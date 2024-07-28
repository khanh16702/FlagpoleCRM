using Common.Enums;
using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using RepositoriesLib;

namespace DataServiceLib
{
    public interface ITemplateService
    {
        List<Template> GetTemplates(string websiteId);
        Template GetTemplateById(int id);
        ResponseModel InsertOrUpdate(Template model);
    }
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _templateRepository;
        public TemplateService(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }
        public List<Template> GetTemplates(string websiteId)
        {
            return _templateRepository.GetTemplates(websiteId);
        }

        public Template GetTemplateById(int id)
        {
            return _templateRepository.GetTemplateById(id);
        }

        public ResponseModel InsertOrUpdate(Template model)
        {
            return _templateRepository.InsertOrUpdate(model);
        }
    }
}
