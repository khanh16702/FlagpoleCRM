using Common.Models;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilize
{
    public class OtpService
    {
        public static string CreateTotp(string secretKey)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(secretKey);
                var totp = new Totp(bytes, 60, OtpHashMode.Sha512, 6);
                return totp.ComputeTotp();
            }
            catch
            {
                return "";
            }
            
        }
    }
}
