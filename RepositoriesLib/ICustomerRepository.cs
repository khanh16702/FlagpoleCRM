using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;

namespace RepositoriesLib
{
    public interface ICustomerRepository
    {
        public ResponseModel InsertAudience(AudienceDTO model);
        public List<Audience> GetAudiences(string websiteId);
        public List<Audience> GetDynamicAudiences();
        public Audience GetAudienceByName(string name, string websiteId);
        public Audience GetAudienceById(string id);
        public ResponseModel DeleteAudience(string id);
        public AudienceCustomer FindAudienceCustomer(string audienceId, string customerId);
        public ResponseModel InsertAudienceCustomer(string audienceId, string customerId);
        public ResponseModel UpdateAudienceCustomer(AudienceCustomer model);
        public List<AudienceCustomer> GetAudienceCustomersByAudienceId(string audienceId);
    }
}