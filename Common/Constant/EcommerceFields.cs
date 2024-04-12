using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Constant
{
    public class EcommerceFields
    {
        // financial status
        public const string FINANCIAL_PAID = "paid";
        public const string FINANCIAL_PENDING = "pending";
        public const string FINANCIAL_REFUND = "refunded";

        // fulfillment status
        public const string FULFILLMENT_FULFILLED = "fulfilled";
        public const string FULFILLMENT_NOTFULFILLED = "notfulfilled";

        // email subscribe state
        public const string EMAIL_NOT_SUBSCRIBED = "not_subscribed";
        public const string EMAIL_SUBSCRIBED = "subscribed";
    }
}
