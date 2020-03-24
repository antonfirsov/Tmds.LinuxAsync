using System;
using System.Globalization;

namespace BenchmarkRunner
{
    public static class Utilities
    {
        public static void ParseIfInt(string line, string match, ref int value)
        {
            if (line.StartsWith(match))
            {
                var subString = line.AsSpan(match.Length).Trim();
                value = int.Parse(subString, NumberStyles.Integer | NumberStyles.AllowThousands);
            }
        }

        public static void ParseIfFloat(string line, string match, ref float value)
        {
            if (line.StartsWith(match))
            {
                value = float.Parse(line.AsSpan(match.Length).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }
    }
}