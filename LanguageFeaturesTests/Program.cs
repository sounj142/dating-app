using System;

namespace LanguageFeaturesTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var dob = new DateTimeOffset(year: 1986, month: 11, day: 28, 
                hour: 0, minute: 0, second: 0, offset: new TimeSpan(-9, 0, 0));


            var now = DateTimeOffset.Now.ToOffset(dob.Offset);

            Console.WriteLine(dob);
            Console.WriteLine(now);

            Console.WriteLine(dob.Date);
            Console.WriteLine(now.Date);

            Console.WriteLine(CalculateAge(dob));
        }

        static int CalculateAge(DateTimeOffset dateOfBirth)
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
