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
    public class WebsiteRepository : IWebsiteRepository
    {
        public List<Website> GetAllWebsites()
        {
            throw new NotImplementedException();
        }

        public Website GetWebsiteByGuid(string id)
        {
            throw new NotImplementedException();
        }

        public List<Website> GetWebsitesByAccountId(string accountId)
        {
            throw new NotImplementedException();
        }

        public ResponseModel Insert(Website model)
        {
            throw new NotImplementedException();
        }

        public ResponseModel Update(Website model)
        {
            throw new NotImplementedException();
        }
    }
}
