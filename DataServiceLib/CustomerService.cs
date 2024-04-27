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
        List<Audience> GetDynamicAudiences();
        Audience GetAudienceByName(string name, string websiteId);
        Audience GetAudienceById(string id);
        ResponseModel DeleteAudience(string id);
        List<AudienceCustomer> GetAudienceCustomersByAudienceId(string audienceId);
        AudienceCustomer FindAudienceCustomer(string audienceId, string customerId);
        ResponseModel InsertAudienceCustomer(string audienceId, string customerId);
        ResponseModel UpdateAudienceCustomer(AudienceCustomer model);

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
                    audience.IsHasModification = model.IsHasModification;
                    audience.ModifiedDate = DateTime.Now;
                    audience.IsDynamic = model.Type == 1 ? true : false;
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

        public List<Audience> GetDynamicAudiences()
        {
            return _flagpoleCRM.Audiences.Where(x => x.IsDynamic && !x.IsDeleted).ToList();
        }

        public Audience GetAudienceByName(string name, string websiteId)
        {
            return _flagpoleCRM.Audiences.FirstOrDefault(x => x.Name == name && x.WebsiteId == websiteId && !x.IsDeleted);
        }

        public Audience GetAudienceById(string id)
        {
            return _flagpoleCRM.Audiences.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
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

        public AudienceCustomer FindAudienceCustomer(string audienceId, string customerId)
        {
            return _flagpoleCRM.AudienceCustomers.FirstOrDefault(x =>
            x.AudienceId == audienceId && x.CustomerId == customerId);
        }

        public ResponseModel InsertAudienceCustomer(string audienceId, string customerId)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var audienceCustomer = new AudienceCustomer
                {
                    AudienceId = audienceId,
                    CustomerId = customerId,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsDeleted = false
                };
                _flagpoleCRM.AudienceCustomers.Add(audienceCustomer);
                _flagpoleCRM.SaveChanges();
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public ResponseModel UpdateAudienceCustomer(AudienceCustomer model)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var audienceCustomer = _flagpoleCRM.AudienceCustomers.FirstOrDefault(x => x.Id == model.Id);
                if (audienceCustomer == null)
                {
                    throw new Exception("AudienceCustomer not found");
                }
                else
                {
                    audienceCustomer.ModifiedDate = DateTime.Now;
                    audienceCustomer.IsDeleted = model.IsDeleted;
                    _flagpoleCRM.AudienceCustomers.Update(audienceCustomer);
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

        public List<AudienceCustomer> GetAudienceCustomersByAudienceId(string audienceId)
        {
            return _flagpoleCRM.AudienceCustomers.Where(x => x.AudienceId == audienceId).ToList();
        }
    }
}
