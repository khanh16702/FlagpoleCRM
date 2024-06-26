﻿using System;
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

        public const string REPORT_TOTAL_ORDERS = "Report:TotalOrders:";
        public const string REPORT_TOTAL_REVENUE = "Report:TotalRevenue:";
        public const string REPORT_RFM = "Report:RFM:";
        public const string REPORT_RFM_RECALCULATE = "Report:RFMRecalculate:";
    }
}
