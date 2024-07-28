using Common.Models;
using FlagpoleCRM.Models;
using log4net;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceLib
{
    public interface IUnsubscribeService
    {
        List<UnsubcribedEmail> GetListUnsubs(string websiteId);
        ResponseModel UpdateUnsubscribe(UnsubcribedEmail model);
        UnsubcribedEmail GetUnsubById(int id);
        UnsubcribedEmail GetUnsubByEmail (string email);
    }
    public class UnsubscribeService : IUnsubscribeService
    {
        private readonly IUnsubscribeRepository _unsubscribeRepository;
        public UnsubscribeService(IUnsubscribeRepository unsubscribeRepository)
        {
            _unsubscribeRepository = unsubscribeRepository;
        }
        public List<UnsubcribedEmail> GetListUnsubs(string websiteId)
        {
            return _unsubscribeRepository.GetListUnsubs(websiteId);
        }
        public ResponseModel UpdateUnsubscribe(UnsubcribedEmail model)
        {
            return _unsubscribeRepository.UpdateUnsubscribe(model);   
        }

        public UnsubcribedEmail GetUnsubById(int id)
        {
            return _unsubscribeRepository.GetUnsubById(id);
        }

        public UnsubcribedEmail GetUnsubByEmail(string email)
        {
            return _unsubscribeRepository.GetUnsubByEmail(email);
        }
    }
}
