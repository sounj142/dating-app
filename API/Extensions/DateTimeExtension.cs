using System;

namespace API.Extensions
{
    public static class DateTimeExtension
    {
        public static int CalculateAge(this DateTimeOffset dateOfBirth)
        {
            var now = DateTimeOffset.Now.ToOffset(dateOfBirth.Offset);

            var today = now.Date;
            var dbo = dateOfBirth.Date;

            // Calculate the age.
            var age = today.Year - dbo.Year;

            // Go back to the year in which the person was born in case of a leap year
            if (dbo.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
