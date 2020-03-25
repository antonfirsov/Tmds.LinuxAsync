using System.Globalization;
using System.Text;

namespace BenchmarkRunner
{
    public enum BenchmarkParameterType
    {
        Argument,
        EnvironmentVariable
    }

    public class BenchmarkParameter
    {
        public string Name { get;  }
        public object[] Values { get;  }

        public bool IsVariable => Values.Length > 1;
        
        public BenchmarkParameterType Type { get; }
        
        public BenchmarkParameter(string name, object[] values, BenchmarkParameterType type)
        {
            Name = name;
            Values = values;
            Type = type;
        }

        public override string ToString()
        {
            StringBuilder bld = new StringBuilder();
            bld.Append(Name);
            bld.Append('=');
            for (int i = 0; i < Values.Length; i++)
            {
                bld.Append($"{Values[i]}");
                if (i < Values.Length - 1) bld.Append(",");
            }

            return bld.ToString();
        }

        public BenchmarkParameterAssignment GetAssignment(int valueIndex) =>
            new BenchmarkParameterAssignment(this, Values[valueIndex]);
    }
}