using System;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime TodayStandardTimezone()
        {
            var nowAtStandardTimezone = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, SystemInfo.StandardTimezone);
            return nowAtStandardTimezone.Date;
        }

        public static int CalculateAge(this DateTime dateOfBirth)
        {
            var today = TodayStandardTimezone();
            var dbo = dateOfBirth.Date;

            // Calculate the age.
            var age = today.Year - dbo.Year;

            // Go back to the year in which the person was born in case of a leap year
            if (dbo.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
