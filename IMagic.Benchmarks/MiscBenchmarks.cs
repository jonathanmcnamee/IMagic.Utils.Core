using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class MiscBenchmarks
{
    // ── ToStringLeadingZero(int, int) ─────────────────────────────────────────
    // Current: builds "d{n}" via string.Format on every call
    // Proposed: pre-cached array covers the only realistic values (1-6 digits)
    private static readonly string[] LeadingZeroFormats = ["d0", "d1", "d2", "d3", "d4", "d5", "d6"];

    [Params(42, 7)]
    public int IntValue;

    [Params(2, 4)]
    public int LeadingZeros;

    [Benchmark(Baseline = true, Description = "ToStringLeadingZero — string.Format per call")]
    public string LeadingZero_Current() =>
        IntValue.ToString(string.Format("d{0}", LeadingZeros));

    [Benchmark(Description = "ToStringLeadingZero — cached array")]
    public string LeadingZero_Proposed() =>
        IntValue.ToString(LeadingZeros < LeadingZeroFormats.Length
            ? LeadingZeroFormats[LeadingZeros]
            : $"d{LeadingZeros}");

    // ── ToStringCommaSeperated: unnecessary .ToArray() ────────────────────────
    private static readonly List<string> SampleList = ["alpha", "beta", "gamma", "delta", "epsilon"];

    [Benchmark(Description = "ToStringCommaSeperated — with ToArray()")]
    public string CommaJoin_Current() =>
        string.Join(",", SampleList.ToArray());

    [Benchmark(Description = "ToStringCommaSeperated — direct IEnumerable")]
    public string CommaJoin_Proposed() =>
        string.Join(",", SampleList);

    // ── Append: string.Format vs interpolation ────────────────────────────────
    private const string Left = "Hello";
    private const string Right = "World";

    [Benchmark(Description = "Append — string.Format")]
    public string Append_Current() =>
        string.Format("{0} {1}", Left, Right);

    [Benchmark(Description = "Append — string interpolation")]
    public string Append_Proposed() =>
        $"{Left} {Right}";

    // ── ToStringFuzzyTime: string.Format vs interpolation ────────────────────
    private static readonly TimeSpan SampleTimeSpan = new TimeSpan(2, 34, 56);

    [Benchmark(Description = "ToStringFuzzyTime — string.Format")]
    public string FuzzyTime_Current() =>
        string.Format("{0:00}:{1:00}:{2:00}", (int)SampleTimeSpan.TotalHours, SampleTimeSpan.Minutes, SampleTimeSpan.Seconds);

    [Benchmark(Description = "ToStringFuzzyTime — interpolation")]
    public string FuzzyTime_Proposed() =>
        $"{(int)SampleTimeSpan.TotalHours:00}:{SampleTimeSpan.Minutes:00}:{SampleTimeSpan.Seconds:00}";
}
