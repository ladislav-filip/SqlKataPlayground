using System;

namespace QueryFilterMongo.Persistence.QueryFilterNs
{
    internal static class StringExtensions
    {
        internal static bool IsTimePeriod(this string value)
        {
            return Enum.TryParse(value, true, out TimePeriod period);
        }

        internal static TimePeriod AsTimePeriod(this string value)
        {
            if (Enum.TryParse(value, true, out TimePeriod period))
            {
                return period;
            }

            throw new ArgumentException(nameof(value) + " is not time period string: " + value);
        }
    }
}