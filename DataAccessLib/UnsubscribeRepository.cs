using Common.Models;
using FlagpoleCRM.Models;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class UnsubscribeRepository : IUnsubscribeRepository
    {
        public List<UnsubcribedEmail> GetListUnsubs(string websiteId)
        {
            throw new NotImplementedException();
        }

        public UnsubcribedEmail GetUnsubByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public UnsubcribedEmail GetUnsubById(int id)
        {
            throw new NotImplementedException();
        }

        public ResponseModel UpdateUnsubscribe(UnsubcribedEmail model)
        {
            throw new NotImplementedException();
        }
    }
}
