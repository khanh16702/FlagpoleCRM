using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CampaignStatisticModel
    {
        public int? TotalCreated { get; set; }
        public int? TotalSent { get; set; }
        public int? TotalEmailsSent { get; set; }
        public int? TotalEmailsSentToday { get; set; }
    }
}
