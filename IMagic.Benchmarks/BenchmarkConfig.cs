using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Columns;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddJob(Job.Default.WithWarmupCount(3).WithIterationCount(10));
        AddExporter(MarkdownExporter.Default);
        AddColumn(StatisticColumn.Mean, StatisticColumn.StdDev, StatisticColumn.Min, StatisticColumn.Max);
    }
}
