using System;
using System.Collections.Generic;

namespace DotNetCoreWebsite
{
    public static class Extensions
    {
        public static long sizeBase = 1024;
        public static double sizeBaseD = (double)sizeBase;

        public static List<(string name, TimeSpan timeSpan)> constTimeSpans = new List<(string name, TimeSpan timeSpan)> {
            ("year", TimeSpan.FromDays(365)),
            ("month", TimeSpan.FromDays(30)),
            ("week", TimeSpan.FromDays(7)),
            ("day", TimeSpan.FromDays(1)),
            ("hour", TimeSpan.FromHours(1)),
            ("minute", TimeSpan.FromMinutes(1)),
            ("second", TimeSpan.FromSeconds(1)),
        };

        public static string ToFileSizeString(this long? length)
        {
            if (length == null)
                return "";

            if (length < sizeBase)
                return $"{length} bytes";
            if (length < sizeBase * sizeBase)
                return string.Format("{0:N2} KiB", (length / sizeBaseD));
            if (length < sizeBase * sizeBase * sizeBase)
                return string.Format("{0:N2} MiB", (length / sizeBaseD / sizeBaseD));
            if (length < sizeBase * sizeBase * sizeBase * sizeBase)
                return string.Format("{0:N2} GiB", (length / sizeBaseD / sizeBaseD / sizeBaseD));

            return "";
        }

        public static string ToAgeString(this TimeSpan? timeSpan)
        {
            if (timeSpan == null)
                return "";

            var timeSpanValue = timeSpan.Value;

            for (int i = 0; i < constTimeSpans.Count; i++)
            {
                var timePair = constTimeSpans[i];

                if (timeSpanValue > timePair.timeSpan)
                {
                    var val = (int)(timeSpanValue / timePair.timeSpan);
                    var hasNextValue = i + 1 < constTimeSpans.Count;

                    string pluralSuffix(int x) => x > 1 ? "s" : "";

                    if (!hasNextValue)
                    {
                        return $"{val} {timePair.name}{pluralSuffix(val)} ago";
                    }
                    else
                    {
                        var nextTimePair = constTimeSpans[i + 1];
                        var nextVal = (int)((timeSpanValue - val * timePair.timeSpan) / nextTimePair.timeSpan);

                        if (nextVal > 0)
                            return $"{val} {timePair.name}{pluralSuffix(val)} {nextVal} {nextTimePair.name}{pluralSuffix(nextVal)} ago";
                        else
                            return $"{val} {timePair.name}{pluralSuffix(val)} ago";
                    }
                }
            }

            return "0 seconds ago";
        }

        public static string NullIfEmpty(this string s)
        {
            if (s == "")
                return null;
            else
                return s;
        }
    }
}
