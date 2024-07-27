using Common.Models;
using FlagpoleCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoriesLib
{
    public interface IWebsiteRepository
    {
        ResponseModel Insert(Website model);
        List<Website> GetWebsitesByAccountId(string accountId);
        Website GetWebsiteByGuid(string id);
        ResponseModel Update(Website model);
        List<Website> GetAllWebsites();
    }
}
