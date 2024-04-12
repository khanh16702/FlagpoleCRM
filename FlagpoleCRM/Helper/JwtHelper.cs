using Common.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FlagpoleCRM.Helper
{
    public class JwtHelper
    {
        public static ResponseModel ValidateToken(string token, IConfiguration _configuration)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Tokens:Key"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
                var id = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                return new ResponseModel() 
                { 
                    IsSuccessful = true, 
                    Data = new JwtModel() 
                    { 
                        Email = email,
                        Id = id
                    } 
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel() { IsSuccessful = false, Message = ex.Message };
            }
        }
    }
}
