using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using Org.BouncyCastle.Math.EC.Rfc7748;
using RepositoriesLib;

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
        private ICustomerRepository _customerRepository;
        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        
        public ResponseModel InsertAudience(AudienceDTO model)
        {
            return _customerRepository.InsertAudience(model);
        }

        public List<Audience> GetAudiences(string websiteId)
        {
            return _customerRepository.GetAudiences(websiteId);
        }

        public List<Audience> GetDynamicAudiences()
        {
            return _customerRepository.GetDynamicAudiences();
        }

        public Audience GetAudienceByName(string name, string websiteId)
        {
            return _customerRepository.GetAudienceByName(name, websiteId);
        }

        public Audience GetAudienceById(string id)
        {
            return _customerRepository.GetAudienceById(id);
        }

        public ResponseModel DeleteAudience(string id)
        {
            return _customerRepository.DeleteAudience(id);
        }

        public AudienceCustomer FindAudienceCustomer(string audienceId, string customerId)
        {
            return _customerRepository.FindAudienceCustomer(audienceId, customerId);
        }

        public ResponseModel InsertAudienceCustomer(string audienceId, string customerId)
        {
            return _customerRepository.InsertAudienceCustomer(audienceId, customerId);
        }

        public ResponseModel UpdateAudienceCustomer(AudienceCustomer model)
        {
            return _customerRepository.UpdateAudienceCustomer(model);
        }

        public List<AudienceCustomer> GetAudienceCustomersByAudienceId(string audienceId)
        {
            return _customerRepository.GetAudienceCustomersByAudienceId(audienceId);
        }
    }
}
