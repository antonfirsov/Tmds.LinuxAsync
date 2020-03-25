namespace BenchmarkRunner
{
    public class BenchmarkParameterAssignment
    {
        public BenchmarkParameterAssignment(BenchmarkParameter parameter, object value)
        {
            Parameter = parameter;
            Value = value;
        }

        public BenchmarkParameter Parameter { get; }
        
        public object Value { get; }

        public bool IsVariable => Parameter.IsVariable;

        public string GetBenchmarkRunnerArgString() => $"--arg \"-{Parameter.Name}={Value}\"";

        public override string ToString() => $"{Parameter.Name}={Value}";
    }
}