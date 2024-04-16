using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extension
{
    public static class TimeExtension
    {
        public static DateTime GetTimeWithOffset(this DateTime time, string timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
            {
                return time;
            }
            var offsetType = timeZone[0] == '+' ? 1 : -1;
            var offsetTime = timeZone.Substring(1);
            var offsetHour = int.Parse(offsetTime.Split(":")[0]);
            var offsetMinute = int.Parse(offsetTime.Split(":")[1]);
            return time.AddHours(offsetType * offsetHour).AddMinutes(offsetType * offsetMinute);
        }
    }
}
