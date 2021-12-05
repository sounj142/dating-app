using System;

namespace API.Helpers
{
    public class ClientInformation
    {
        public TimeSpan TimeZoneOffset { get; private set; } = new TimeSpan(0, 0, 0); // default UTC timezone

        public void SetTimeZoneOffset(int clientTimezoneOffset)
        {
            TimeZoneOffset = new TimeSpan(0, -clientTimezoneOffset, 0);
        }

        public DateTimeOffset GetClientNow()
        {
            return DateTimeOffset.UtcNow.ToOffset(TimeZoneOffset);
        }
    }
}
