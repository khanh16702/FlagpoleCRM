using Common.Models;
using DataServiceLib;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private readonly ILog _log;
        public AccountAPIController(IConfiguration configuration, IAccountService accountService, ILog log)
        {
            _configuration = configuration;
            _accountService = accountService;
            _log = log;
        }

        [Route("GetAccountByEmail")]
        [HttpGet]
        public Account GetAccountByEmail(string email)
        {
            return _accountService.GetAccountByEmail(email);
        }

        [Route("GetAccountById")]
        [HttpGet]
        public Account GetAccountById(string id)
        {
            return _accountService.GetAccountById(id);
        }

        [Route("UpdateAccount")]
        [HttpPost]
        public ResponseModel UpdateAccount(Account account)
        {
            return _accountService.Update(account);
        }
    }
}
