using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace API
{
    public static class SystemInfo
    {
        private static TimeZoneInfo _standardTimezone = TimeZoneInfo.Local;

        public static TimeZoneInfo StandardTimezone => _standardTimezone;

        public static void ReadStandardTimezoneFromAppseting(IConfiguration configuration)
        {
            var timezoneName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? configuration.GetValue<string>("StandardTimezoneOnWindows")
                : configuration.GetValue<string>("StandardTimezoneOnLinux");

            _standardTimezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
        }
    }
}
