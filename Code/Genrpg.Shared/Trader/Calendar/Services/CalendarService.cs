using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Calendar.Constants;
using Genrpg.Shared.Utils;
using System.Text;

namespace Genrpg.Shared.Trader.Calendar.Services
{
    public interface ICalendarService : IInjectable
    {
        string PrintDay(long day);
    }

    public class CalendarService : ICalendarService
    {
        public string PrintDay(long totalDays)
        {
            if (totalDays < 0)
            {
                totalDays = 0;
            }

            long dayOfWeek = totalDays % CalendarConstants.DaysPerWeek;

            totalDays /= CalendarConstants.DaysPerWeek;

            long weekOfMonth = totalDays % CalendarConstants.WeeksPerMonth;

            totalDays /= CalendarConstants.WeeksPerMonth;

            long monthOfYear = totalDays % CalendarConstants.MonthsPerYear;

            totalDays /= CalendarConstants.MonthsPerYear;


            StringBuilder sb = new StringBuilder();

            sb.Append("It is the " + (dayOfWeek + 1) + NumberUtils.GetOrdinalSuffix(dayOfWeek + 1) + " day");
            sb.Append(" of the " + (weekOfMonth + 1) + NumberUtils.GetOrdinalSuffix(weekOfMonth + 1) + " week");
            sb.Append(" of the " + (monthOfYear + 1) + NumberUtils.GetOrdinalSuffix(monthOfYear + 1) + " month");
            sb.Append(" of the year " + (totalDays + CalendarConstants.StartYear));

            return sb.ToString();

        }
    }
}
