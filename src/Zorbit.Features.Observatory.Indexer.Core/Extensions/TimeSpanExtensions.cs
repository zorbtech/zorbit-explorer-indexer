using System;
using System.Text;

namespace Zorbit.Features.Observatory.Core.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string Pretty(this TimeSpan span)
        {
            if (span == TimeSpan.Zero)
                return "0m";

            var sb = new StringBuilder();
            if (span.Days > 0)
            {
                sb.AppendFormat("{0}d ", span.Days);
            }

            if (span.Hours > 0)
            {
                sb.AppendFormat("{0}h ", span.Hours);
            }

            if (span.Minutes > 0)
            {
                sb.AppendFormat("{0}m", span.Minutes);
            }

            if (span.Seconds > 0)
            {
                sb.AppendFormat("{0}s", span.Seconds);
            }

            var result = sb.ToString();
            return result == string.Empty ? "< 1min" : result;
        }
    }
}
