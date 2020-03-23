using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BenchmarkRunner
{
    class CsvWriter : IDisposable
    {
        private readonly StreamWriter _streamWriter;
        
        private readonly List<object> _currentLine = new List<object>();

        public CsvWriter(string path)
        {
            _streamWriter = new StreamWriter(path);
        }

        public void Append<T>(T val) => _currentLine.Add(val);

        public void AppendRange<T>(IEnumerable<T> vals) => _currentLine.AddRange(vals.Cast<object>());

        public void EndLine()
        {
            for (int i = 0; i < _currentLine.Count; i++)
            {
                _streamWriter.Write(_currentLine[i]);
                if (i < _currentLine.Count - 1) _streamWriter.Write(", ");
            }
            _streamWriter.WriteLine();
            _streamWriter.Flush();
            _currentLine.Clear();
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }
    }
}