using Common.Models;
using Common.Utilize;
using FlagpoleCRM.DTO;
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
    public interface IAccountService
    {
        ResponseModel Insert(AccountDTO model);
        Account GetAccountByEmail(string email);
        Account GetAccountById(string id);
        ResponseModel Update(Account model);
    }
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public ResponseModel Insert(AccountDTO model)
        {
            return _accountRepository.Insert(model);
        }

        public Account GetAccountByEmail(string email)
        {
            return _accountRepository.GetAccountByEmail(email);
        }

        public Account GetAccountById(string id)
        {
            return _accountRepository.GetAccountById(id);
        }

        public ResponseModel Update(Account model)
        {
            return _accountRepository.Update(model);
        }
    }
}
