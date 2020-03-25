using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BenchmarkRunner
{
    public static class BenchmarkRunnerUtilities
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
        
        public static string GetBenchmarkRunnerParameterString(this IReadOnlyList<BenchmarkParameterAssignment> line)
        {
            StringBuilder bld = new StringBuilder();
            
            for (int i = 0; i < line.Count; i++)
            {
                BenchmarkParameterAssignment p = line[i];
                if (i > 0) bld.Append(' ');
                bld.Append(p.GetBenchmarkRunnerArgString());
            }

            return bld.ToString();
        }
    }
}