using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoriesLib
{
    public interface IAccountRepository
    {
        ResponseModel Insert(AccountDTO model);
        Account GetAccountByEmail(string email);
        Account GetAccountById(string id);
        ResponseModel Update(Account model);
    }
}
