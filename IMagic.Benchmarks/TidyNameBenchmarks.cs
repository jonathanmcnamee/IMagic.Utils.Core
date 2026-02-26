using BenchmarkDotNet.Attributes;
using System.Text;

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class TidyNameBenchmarks
{
    [Params("john smith", "mary anne jones", "JONATHAN MC NAMEE")]
    public string Name = "";

    // Current: string concat with += in a loop — allocates a new string per word-part
    [Benchmark(Baseline = true, Description = "Current (string += in loop)")]
    public string Current()
    {
        string tidyName = string.Empty;
        foreach (string namePart in Name.Split(' '))
        {
            tidyName += namePart[0].ToString().ToUpper();
            tidyName += namePart.Substring(1) + " ";
        }
        return tidyName.Trim();
    }

    // Proposed: StringBuilder — one buffer, no intermediate string allocations
    [Benchmark(Description = "Proposed (StringBuilder)")]
    public string Proposed()
    {
        var sb = new StringBuilder();
        foreach (string namePart in Name.Split(' '))
        {
            sb.Append(char.ToUpper(namePart[0]));
            sb.Append(namePart, 1, namePart.Length - 1);
            sb.Append(' ');
        }
        if (sb.Length > 0) sb.Length--; // trim trailing space
        return sb.ToString();
    }
}
