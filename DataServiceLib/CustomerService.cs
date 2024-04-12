using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace DataServiceLib
{
    public interface ICustomerService
    {
        ResponseModel InsertAudience(AudienceDTO model);
        List<Audience> GetAudiences(string websiteId);
        Audience GetAudienceByName(string name, string websiteId);

        ResponseModel DeleteAudience(string id);
    }
    public class CustomerService : ICustomerService
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public CustomerService(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }
        
        public ResponseModel InsertAudience(AudienceDTO model)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                if (model.Id == "0")
                {
                    var audience = new Audience
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = model.Name,
                        Description = model.Description,
                        IsHasModification = true,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = null,
                        IsDynamic = model.Type == 1 ? true : false,
                        Sqlquery = model.Sqlquery,
                        ElasticQuery = model.ElasticQuery,
                        RulesQueryBuilder = model.RulesQueryBuilder,
                        Limit = model.Limit,
                        IsDeleted = false,
                        WebsiteId = model.WebsiteId
                    };
                    _flagpoleCRM.Audiences.Add(audience);
                    _flagpoleCRM.SaveChanges();
                }
                else
                {
                    var audience = _flagpoleCRM.Audiences.FirstOrDefault(x => x.Id == model.Id);
                    audience.Name = model.Name;
                    audience.Description = model.Description;
                    audience.IsHasModification = true;
                    audience.ModifiedDate = DateTime.Now;
                    audience.IsDynamic = model.IsDynamic;
                    audience.Sqlquery = model.Sqlquery;
                    audience.ElasticQuery = model.ElasticQuery;
                    audience.RulesQueryBuilder = model.RulesQueryBuilder;
                    audience.Limit = model.Limit;
                    audience.IsDeleted = false;
                    audience.WebsiteId = model.WebsiteId;
                    _flagpoleCRM.Audiences.Update(audience);
                    _flagpoleCRM.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public List<Audience> GetAudiences(string websiteId)
        {
            return _flagpoleCRM.Audiences.Where(x => x.WebsiteId == websiteId && !x.IsDeleted).ToList();
        }

        public Audience GetAudienceByName(string name, string websiteId)
        {
            return _flagpoleCRM.Audiences.FirstOrDefault(x => x.Name == name && x.WebsiteId == websiteId && !x.IsDeleted);
        }

        public ResponseModel DeleteAudience(string id)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var audience = _flagpoleCRM.Audiences.FirstOrDefault(x => x.Id == id);
                audience.IsDeleted = true;
                _flagpoleCRM.Audiences.Update(audience);
                _flagpoleCRM.SaveChanges();
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
