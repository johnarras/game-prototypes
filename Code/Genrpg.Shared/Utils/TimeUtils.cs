using System.Text;

namespace Genrpg.Shared.Utils
{
    public static class TimeUtils
    {
        public static string PrintTime(long totalSeconds)
        {

            StringBuilder sb = new StringBuilder();
            long seconds = totalSeconds % 60;
            totalSeconds /= 60;
            long minutes = totalSeconds % 60;
            totalSeconds /= 60;
            long hours = totalSeconds % 24;

            long days = totalSeconds / 24;

            int itemsShown = 0;
            if (days > 0)
            {
                sb.Append(days + "d ");
                itemsShown++;
            }
            if (hours > 0)
            {
                sb.Append(hours + "h ");
                itemsShown++;
            }
            if (minutes > 0 && itemsShown < 2)
            {
                sb.Append(minutes + "m ");
                itemsShown++;
            }
            if (seconds > 0 && itemsShown < 2)
            {
                sb.Append(seconds + "s");
            }
            return sb.ToString();
        }
    }
}

