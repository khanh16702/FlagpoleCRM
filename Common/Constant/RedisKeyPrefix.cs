using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Constant
{
    public class RedisKeyPrefix
    {
        public const string OTP_EMAIL = "Verification:OTP:Email:";
        public const string CAMPAIGN_EMAIL_TOTAL = "Campaign:Email:TotalCreated:";
        public const string CAMPAIGN_EMAIL_SENT = "Campaign:Email:TotalSent:";
        public const string CAMPAIGN_EMAIL_EMAILSSENT = "Campaign:Email:TotalEmailsSent:";
    }
}
