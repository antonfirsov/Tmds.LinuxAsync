using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenchmarkRunner
{
    public class BenchmarkParameterSet : IReadOnlyList<BenchmarkParameter>
    {
        private readonly IReadOnlyList<BenchmarkParameter> _parameters;

        internal BenchmarkParameterSet(IReadOnlyList<BenchmarkParameter> parameters)
        {
            _parameters = parameters;
        }

        public IEnumerable<char> Names => _parameters.Select(p => p.Name);

        public IEnumerable<char> VariableNames => _parameters.Where(p => p.IsVariable).Select(p => p.Name);

        public BenchmarkParameter this[char name] => _parameters.First(p => p.Name == name);
        
        public IEnumerator<BenchmarkParameter> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _parameters.GetEnumerator();

        public static BenchmarkParameterSet Parse(string parameters, string environmentVariables)
        {
            return Parse(parameters.Split(' '));
        }
        
        private static BenchmarkParameterSet Parse(string[] args)
        {
            List<BenchmarkParameter> result = new List<BenchmarkParameter>();
            
            foreach (string p in args.Where(s => s.Contains('=')))
            {
                int sep = p.IndexOf('=');
                string[] nv = p.Split('=');
                string name = nv[0];
                string vals = nv[1];

                name = name.TrimStart('-');

                object[] values;

                int rangeIdx = vals.IndexOf("..", StringComparison.InvariantCulture);
                if (rangeIdx >= 0)
                {
                    int start = int.Parse(vals.AsSpan(0, rangeIdx));
                    int end = int.Parse(vals.AsSpan(rangeIdx + 2));

                    values = Enumerable.Range(start, end - start + 1).Cast<object>().ToArray();
                }
                else
                {
                    values = vals.Split(',').Cast<object>().ToArray();
                }

                result.Add(new BenchmarkParameter(name[0], values));
            }
            
            return new BenchmarkParameterSet(result);
        }

        public int Count => _parameters.Count;

        public BenchmarkParameter this[int index] => _parameters[index];

        public override string ToString() => string.Join('\n', _parameters);

        /// <summary>
        /// List all possible configurations.
        /// </summary>
        public IEnumerable<IReadOnlyList<BenchmarkParameterAssignment>> CartesianProduct()
        {
            using CartesianEnumerator enumerator = new CartesianEnumerator(_parameters);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public string GetBenchmarkRunnerParameterStringForLine(IReadOnlyList<BenchmarkParameterAssignment> line)
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

        struct CartesianEnumerator : IEnumerator<IReadOnlyList<BenchmarkParameterAssignment>>
        {
            private int _position;
            private readonly IReadOnlyList<BenchmarkParameter> _parameters;
            private BenchmarkParameterAssignment[] _currentLine;
            private int _max;
            
            public CartesianEnumerator(IReadOnlyList<BenchmarkParameter> parameters)
            {
                _parameters = parameters;
                _position = 0;
                _currentLine = null;
                _max = 1;
                
                foreach (BenchmarkParameter p in _parameters)
                {
                    _max *= p.Values.Length;
                }
            }
            
            public bool MoveNext()
            {
                _currentLine = FetchCurrentLine();
                _position++;
                return _position <= _max;
            }

            public void Reset()
            {
                _position = 0;
                _currentLine = null;
            }

            public IReadOnlyList<BenchmarkParameterAssignment> Current => _currentLine;

            object? IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            private BenchmarkParameterAssignment[] FetchCurrentLine()
            {
                var result = new BenchmarkParameterAssignment[_parameters.Count];
                int idx = _position;
                
                for (int i = _parameters.Count - 1; i >= 0; i--)
                {
                    BenchmarkParameter p = _parameters[i];
                    int valueIdx = idx % p.Values.Length;
                    result[i] = p.GetAssignment(valueIdx);
                    idx /= p.Values.Length;
                }

                return result;
            }
        }
    }
}