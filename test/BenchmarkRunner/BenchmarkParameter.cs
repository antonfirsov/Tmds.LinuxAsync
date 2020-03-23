using System.Globalization;
using System.Text;

namespace BenchmarkRunner
{
    public class BenchmarkParameter
    {
        public char Name { get;  }
        public object[] Values { get;  }

        public bool IsVariable => Values.Length > 1;
        
        public BenchmarkParameter(char name, object[] values)
        {
            Name = name;
            Values = values;
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
    }
}