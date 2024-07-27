using Common.Models;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class AccountRepository : IAccountRepository
    {
        public Account GetAccountByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Account GetAccountById(string id)
        {
            throw new NotImplementedException();
        }

        public ResponseModel Insert(AccountDTO model)
        {
            throw new NotImplementedException();
        }

        public ResponseModel Update(Account model)
        {
            throw new NotImplementedException();
        }
    }
}
