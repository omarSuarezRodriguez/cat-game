using System;

namespace WhiskerHaven.Utils
{
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string Format(double value)
        {
            if (value < 0) return "-" + Format(-value);
            if (value < 1000) return value.ToString("0.#");

            int tier = (int)Math.Log10(Math.Abs(value)) / 3;
            tier = Math.Min(tier, Suffixes.Length - 1);

            double scaled = value / Math.Pow(1000, tier);
            string suffix = Suffixes[tier];

            return scaled < 10
                ? scaled.ToString("0.00") + suffix
                : scaled < 100
                    ? scaled.ToString("0.0") + suffix
                    : scaled.ToString("0") + suffix;
        }

        public static string FormatTime(double seconds)
        {
            if (seconds < 60) return $"{(int)seconds}s";
            if (seconds < 3600) return $"{(int)(seconds / 60)}m {(int)(seconds % 60)}s";
            if (seconds < 86400)
            {
                int h = (int)(seconds / 3600);
                int m = (int)((seconds % 3600) / 60);
                return $"{h}h {m}m";
            }
            int d = (int)(seconds / 86400);
            int hr = (int)((seconds % 86400) / 3600);
            return $"{d}d {hr}h";
        }

        public static string FormatCurrency(double value, string symbol = "")
        {
            return symbol + Format(value);
        }
    }
}
