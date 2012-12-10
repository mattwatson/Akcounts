using System;
using System.Collections.Generic;

namespace Akcounts.UI.Util
{
    class DateUtil
    {
        public static IList<DateTime> GenerateDateTimeRange(DateTime fromDate, DateTime toDate)
        {
            var result = new List<DateTime>();
            var date = fromDate <= toDate ? fromDate : toDate;
            var endDate = fromDate <= toDate ? toDate : fromDate;

            while (date <= endDate)
            {
                result.Add(date);
                date = date.AddDays(1);
            }

            return result;
        }
    }
}
