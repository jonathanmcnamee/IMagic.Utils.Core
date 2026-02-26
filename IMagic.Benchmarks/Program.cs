using BenchmarkDotNet.Running;
using System.Reflection;

if (args.Length > 0 && args[0] == "correctness")
{
    CorrectnessCheck.Run();
}
else if (args.Length > 0 && args[0] == "all")
{
    BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).RunAll();
}
else
{
    // Run only the new audit benchmarks (ToStringDecimalPlaces results already captured)
    BenchmarkRunner.Run<TidyNameBenchmarks>();
    BenchmarkRunner.Run<RegexBenchmarks>();
    BenchmarkRunner.Run<MiscBenchmarks>();
}

static class CorrectnessCheck
{
    static readonly string[] DecimalFormats = ["0", "0.0", "0.00", "0.000", "0.0000"];

    static string Current(double item, int totalDecimalPlaces)
    {
        string suffix = "";
        for (int i = 0; i < totalDecimalPlaces; i++) suffix += "0";
        string fmt = string.Format("0.{0}", suffix);
        return item.ToString(fmt);
    }

    static string Cached(double item, int totalDecimalPlaces)
    {
        string fmt = totalDecimalPlaces < DecimalFormats.Length
            ? DecimalFormats[totalDecimalPlaces]
            : "0." + new string('0', totalDecimalPlaces);
        return item.ToString(fmt);
    }

    public static void Run()
    {
        var cases = new (double v, int dp)[]
        {
            (1.23456789, 0), (1.23456789, 2), (1.23456789, 4),
            (0.0, 2), (-42.5, 2), (999.99, 0), (999.99, 2),
            (1.005, 2), (0.0, 0), (123.456, 6), (double.MaxValue, 2),
            (double.MinValue, 2), (0.000001, 4), (-0.0001, 3)
        };

        bool allMatch = true;
        foreach (var (v, dp) in cases)
        {
            string cur = Current(v, dp);
            string cac = Cached(v, dp);
            string status = cur == cac ? "OK  " : "DIFF";
            if (cur != cac) allMatch = false;
            Console.WriteLine($"{status} | v={v,22} dp={dp} | current='{cur}' cached='{cac}'");
        }
        Console.WriteLine(allMatch ? "\nAll outputs match." : "\nDIFFERENCES FOUND.");
    }
}
