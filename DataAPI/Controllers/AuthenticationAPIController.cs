using Common.Models;
using Common.Utilize;
using DataServiceLib;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using StackExchange.Redis;
using Common.Constant;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace DataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private readonly ILog _log;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDb;
        public AuthenticationAPIController(IConfiguration configuration, 
            IAccountService accountService, 
            ILog log, 
            IConnectionMultiplexer connectionMultiplexer)
        {
            _configuration = configuration;
            _accountService = accountService;
            _log = log;
            _connectionMultiplexer = connectionMultiplexer;
            _redisDb = _connectionMultiplexer.GetDatabase(0);
        }

        [Route("Register")]
        [HttpPost]
        public async Task<ResponseModel> Register(AccountDTO model)
        {
            var response = new ResponseModel();

            if (_accountService.GetAccountByEmail(model.Email) != null)
            {
                response.IsSuccessful = false;
                response.Message = "This email has been registered!";
                return response;
            }

            response = _accountService.Insert(model);
            if (!response.IsSuccessful)
            {
                return response;
            }

            response = await SendOTP(model.Email);

            if (!response.IsSuccessful)
            {
                response.Message = "Error while trying to send verification email. Please try again later";
            }
            return response;
        }

        [Route("ConfirmOTP")]
        [HttpPost]
        public async Task<ResponseModel> ConfirmOTP(ConfirmEmailDTO model)
        {
            var key = RedisKeyPrefix.OTP_EMAIL + model.Email;
            if (_redisDb.StringGet(key) == RedisValue.Null)
            {
                return new ResponseModel()
                {
                    IsSuccessful = true,
                    Message = "Failed"
                };
            }

            var otp = await _redisDb.StringGetAsync(key);
            if (otp != model.Code)
            {
                return new ResponseModel()
                {
                    IsSuccessful = true,
                    Message = "Failed"
                };
            }

            await _redisDb.StringGetDeleteAsync(key);

            var account = _accountService.GetAccountByEmail(model.Email);
            account.EmailConfirmed = true;
            _accountService.Update(account);
            return new ResponseModel()
            {
                IsSuccessful = true,
            };
        }

        [Route("SendOTP")]
        [HttpGet]
        public async Task<ResponseModel> SendOTP(string accountEmail)
        {
            var response = new ResponseModel();

            var emailBody = "";
            try
            {
                using (StreamReader stream = System.IO.File.OpenText("Content/template-mail/ConfirmEmail.html"))
                {
                    emailBody = stream.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                return response;
            }

            var account = _accountService.GetAccountByEmail(accountEmail);
            if (string.IsNullOrEmpty(account.SecretKey))
            {
                response.Message = "No secret key found";
                response.IsSuccessful = false;
                return response;
            }

            var totpCode = OtpService.CreateTotp(account.SecretKey);
            if (string.IsNullOrEmpty(totpCode))
            {
                response.IsSuccessful = false;
                response.Message = "Error when trying to send OTP. Please try again later";
                return response;
            }

            var key = RedisKeyPrefix.OTP_EMAIL + account.Email;
            if (_redisDb.StringGet(key) != RedisValue.Null)
            {
                await _redisDb.StringGetDeleteAsync(key);
            }
            await _redisDb.StringSetAsync(key, totpCode, TimeSpan.FromMinutes(1));

            emailBody = emailBody.Replace("#VERIFICATION_CODE#", totpCode);
            var email = _configuration["EmailSender:Email"];
            var to = account.Email;
            var subject = "[FlagpoleCRM] Verify your email now!";
            var body = emailBody;
            var password = _configuration["EmailSender:Password"];
            var smtpServer = _configuration["EmailSender:SMTPServer"];
            var port = int.Parse(_configuration["EmailSender:Port"]);

            var emailBuildResult = EmailService.EmailBuilder(email, to, subject, body, password, smtpServer, port);
            var request = (EmailDTO)emailBuildResult.Data.GetType().GetProperty("request").GetValue(emailBuildResult.Data, null);
            var smtp = (SmtpDTO)emailBuildResult.Data.GetType().GetProperty("smtp").GetValue(emailBuildResult.Data, null);

            response = EmailService.SendEmail(request, smtp);
            return response;
        }

        [Route("Login")]
        [HttpPost]
        public async Task<ResponseModel> Login(AccountDTO model)
        {
            var account = _accountService.GetAccountByEmail(model.Email);
            if (account == null)
            {
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Message = "Email or password is incorrect"
                };
            }
            if (!EncodeAction.Verify(model.Password, account.Password, account.Salt))
            {
                return new ResponseModel
                {
                    IsSuccessful = true,
                    Message = "Email or password is incorrect"
                };
            }
            if (!account.EmailConfirmed)
            {
                return await SendOTP(account.Email);
            }

            return new ResponseModel
            {
                IsSuccessful = true,
                Data = account.Email
            };
        }

        [Route("CreateToken")]
        [HttpPost]
        public ResponseModel CreateToken(Account account)
        {
            try
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id),
                    new Claim(ClaimTypes.Email, account.Email)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                var credits = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: credits);
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return new ResponseModel() { IsSuccessful = true, Data =  jwt};
            }
            catch (Exception ex)
            {
                return new ResponseModel() { IsSuccessful = false, Message = ex.Message };
            }
        }
    }
}
