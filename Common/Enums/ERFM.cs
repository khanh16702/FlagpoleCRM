using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum ERFM
    {
        [Description("Champions")]
        Champions = 1,
        [Description("Loyal Customers")]
        Loyal = 2,
        [Description("Potential Loyalist")]
        Potential = 3,
        [Description("Recent Customers")]
        Recent = 4,
        [Description("Promising")]
        Promising = 5,
        [Description("Customers Needing Attention")]
        NeedingAttention = 6,
        [Description("About To Sleep")]
        AboutToSleep = 7,
        [Description("At Risk")]
        AtRisk = 8,
        [Description("Can’t Lose Them")]
        CantLoseThem = 9,
        [Description("Hibernating")]
        Hibernating = 10,
        [Description("Lost")]
        Lost = 11
    }
}
