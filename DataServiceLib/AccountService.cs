using Common.Models;
using Common.Utilize;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
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
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public AccountService(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }
        public ResponseModel Insert(AccountDTO model)
        {
            var error = "";
            try
            {
                var salt = RandomGenerating.RandomSecretString(64);
                var account = new Account()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    Password = EncodeAction.Hash(model.Password, salt),
                    FullName = RandomGenerating.RandomString(8),
                    Avatar = "/img/default-avatar/UserDefault.png",
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    IsDeleted = false,
                    SecretKey = RandomGenerating.RandomSecretString(64),
                    Salt = salt,
                    Timezone = "+07:00"
                };

                _flagpoleCRM.Accounts.Add(account);
                _flagpoleCRM.SaveChanges();
            }
            catch(Exception e)
            {
                _log.Error("Error when insert account: " + error, e);
                error = e.Message;
            }

            var response = new ResponseModel();
            if (string.IsNullOrEmpty(error))
            {
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.Message = error;
            }
            return response;
        }

        public Account GetAccountByEmail(string email)
        {
            return _flagpoleCRM.Accounts.FirstOrDefault(x => x.Email == email);
        }

        public Account GetAccountById(string id)
        {
            return _flagpoleCRM.Accounts.FirstOrDefault(x => x.Id == id);
        }

        public ResponseModel Update(Account model)
        {
            var error = "";
            try
            {
                var account = GetAccountById(model.Id);
                if (account == null)
                {
                    return new ResponseModel() { IsSuccessful = false, Message = "Cannot find matching account" };
                }
                account.Avatar = string.IsNullOrEmpty(model.Avatar) ? account.Avatar : model.Avatar;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    account.Password = model.Password != account.Password ? EncodeAction.Hash(model.Password, account.Salt) : account.Password;
                }
                account.FullName = model.FullName ?? account.FullName;
                account.Timezone = model.Timezone ?? account.Timezone;
                account.IsDeleted = model.IsDeleted != account.IsDeleted ? model.IsDeleted : account.IsDeleted;
                account.EmailConfirmed = model.EmailConfirmed != account.EmailConfirmed ? model.EmailConfirmed : account.EmailConfirmed;
                account.PhoneNumberConfirmed = model.PhoneNumberConfirmed != account.PhoneNumberConfirmed ? model.PhoneNumberConfirmed : account.PhoneNumberConfirmed;
                account.PhoneNumber = model.PhoneNumber ?? account.PhoneNumber;

                _flagpoleCRM.Accounts.Update(account);
                _flagpoleCRM.SaveChanges();
            }
            catch(Exception ex)
            {
                _log.Error("Error when update account: " + error, ex);
                error = ex.Message;
            }

            var response = new ResponseModel();
            if (string.IsNullOrEmpty(error))
            {
                response.IsSuccessful = true;
            }
            else
            {
                response.IsSuccessful = false;
                response.Message = error;
            }
            return response;
        }
    }
}
