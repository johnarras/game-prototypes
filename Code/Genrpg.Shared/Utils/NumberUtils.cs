namespace Genrpg.Shared.Utils
{
    public static class NumberUtils
    {
        public static string GetOrdinalSuffix(long number)
        {

            long hundrethsRemainder = number % 100;
            if (hundrethsRemainder >= 11 && hundrethsRemainder <= 13)
            {
                return "th";
            }

            if (number % 10 == 1)
            {
                return "st";
            }
            else if (number % 10 == 2)
            {
                return "nd";
            }
            else if (number % 10 == 3)
            {
                return "rd";
            }

            return "th";
        }
    }
}
