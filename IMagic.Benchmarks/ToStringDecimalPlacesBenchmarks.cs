using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class ToStringDecimalPlacesBenchmarks
{
    // Pre-cached format strings for the proposed version
    private static readonly string[] DecimalFormats = ["0", "0.0", "0.00", "0.000", "0.0000"];

    [Params(1.23456789, 999.99, 0.0, -42.5)]
    public double Value;

    [Params(0, 2, 4)]
    public int DecimalPlaces;

    [Benchmark(Baseline = true, Description = "Current (string concat loop)")]
    public string Current()
    {
        string suffix = "";
        for (int i = 0; i < DecimalPlaces; i++)
        {
            suffix += "0";
        }
        string formatString = string.Format("0.{0}", suffix);
        return Value.ToString(formatString);
    }

    [Benchmark(Description = "Cached (pre-built format array)")]
    public string Cached()
    {
        string formatString = DecimalPlaces < DecimalFormats.Length
            ? DecimalFormats[DecimalPlaces]
            : "0." + new string('0', DecimalPlaces);
        return Value.ToString(formatString);
    }
}
