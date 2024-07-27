using Common.Models;
using FlagpoleCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoriesLib
{
    public interface IUnsubscribeRepository
    {
        List<UnsubcribedEmail> GetListUnsubs(string websiteId);
        ResponseModel UpdateUnsubscribe(UnsubcribedEmail model);
        UnsubcribedEmail GetUnsubById(int id);
        UnsubcribedEmail GetUnsubByEmail(string email);
    }
}
