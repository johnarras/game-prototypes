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

            long dayOfMonth = totalDays % CalendarConstants.DaysPerMonth;

            long monthCount = totalDays / CalendarConstants.DaysPerMonth;    

            long monthOfYear = monthCount % CalendarConstants.MonthsPerYear;

            long yearStart = monthCount / CalendarConstants.MonthsPerYear + CalendarConstants.StartYear;


            StringBuilder sb = new StringBuilder();

            sb.Append("The " + (dayOfMonth + 1) + NumberUtils.GetOrdinalSuffix(dayOfMonth + 1) + " of " +
                "M" + (monthOfYear + 1) + " in " + yearStart);
            return sb.ToString();

        }
    }
}
