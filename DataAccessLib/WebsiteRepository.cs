using Common.Models;
using FlagpoleCRM.Models;
using log4net;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class WebsiteRepository : IWebsiteRepository
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public WebsiteRepository(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }

        public List<Website> GetAllWebsites()
        {
            return _flagpoleCRM.Websites.ToList();
        }

        public Website GetWebsiteByGuid(string id)
        {
            return _flagpoleCRM.Websites.FirstOrDefault(x => x.Guid == id && !x.IsDeleted);
        }

        public List<Website> GetWebsitesByAccountId(string accountId)
        {
            return _flagpoleCRM.Websites.Where(x => x.AccountId == accountId && !x.IsDeleted).ToList();
        }

        public ResponseModel Insert(Website model)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            try
            {
                model.Guid = Guid.NewGuid().ToString();
                model.WebsiteType = 1;
                model.CreatedDate = DateTime.UtcNow;
                model.IsDeleted = false;
                model.ShopifyStore = "";
                model.ShopifyToken = "";
                model.HaravanToken = "";
                _flagpoleCRM.Add(model);
                _flagpoleCRM.SaveChanges();
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccessful = false;
            }
            return response;
        }

        public ResponseModel Update(Website model)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            try
            {
                var website = _flagpoleCRM.Websites.FirstOrDefault(x => x.Guid == model.Guid);
                if (website == null)
                {
                    response.IsSuccessful = false;
                    response.Message = "Website could not be found";
                }
                else
                {
                    website.IsDeleted = model.IsDeleted;
                    website.ShopifyStore = !string.IsNullOrEmpty(model.ShopifyStore) ? model.ShopifyStore : website.ShopifyStore;
                    website.ShopifyToken = !string.IsNullOrEmpty(model.ShopifyToken) ? model.ShopifyToken : website.ShopifyToken;
                    website.HaravanToken = !string.IsNullOrEmpty(model.HaravanToken) ? model.HaravanToken : website.HaravanToken;
                    _flagpoleCRM.Websites.Update(website);
                    _flagpoleCRM.SaveChanges();
                }

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
