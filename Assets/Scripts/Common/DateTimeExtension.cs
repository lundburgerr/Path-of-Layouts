using System;
using System.Globalization;

namespace fireMCG.PathOfLayouts.Common
{
    public static class DateTimeExtension
    {
        public static string ToIsoUtc(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("O");
        }

        public static bool TryParseIsoUtc(string isoString, out DateTime result)
        {
            return DateTime.TryParseExact(
                isoString,
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out result);
        }
    }
}