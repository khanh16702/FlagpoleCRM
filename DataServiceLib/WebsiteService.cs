using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceLib
{
    public interface IWebsiteService
    {
        ResponseModel Insert(Website model);
        List<Website> GetWebsitesByAccountId(string accountId);
        Website GetWebsiteByGuid(string id);
        ResponseModel Update(Website model);
        List<Website> GetAllWebsites();
    }
    public class WebsiteService : IWebsiteService
    {
        private readonly IWebsiteRepository _websiteRepository;
        public WebsiteService(IWebsiteRepository websiteRepository, ILog log)
        {
            _websiteRepository = websiteRepository;
        }
        public Website GetWebsiteByGuid(string id)
        {
            return _websiteRepository.GetWebsiteByGuid(id);
        }

        public List<Website> GetWebsitesByAccountId(string accountId)
        {
            return _websiteRepository.GetWebsitesByAccountId(accountId);
        }

        public ResponseModel Insert(Website model)
        {
            return _websiteRepository.Insert(model);   
        }

        public ResponseModel Update(Website model)
        {
            return _websiteRepository.Update(model);
        }

        public List<Website> GetAllWebsites()
        {
            return _websiteRepository.GetAllWebsites();
        }
    }
}
