using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using System.Collections.Generic;
using System.Linq;

namespace HLab.Geo.Benchmark
{
    public class BenchmarkConfig
    {
        /// <summary>
        /// Get a custom configuration
        /// </summary>
        /// <returns></returns>
        public static IConfig Get()
        {
            return ManualConfig.CreateEmpty()
                // Jobs
                .AddJob(Job.Default
                    .WithRuntime(CoreRuntime.Core90)
                    .WithPlatform(Platform.X64))
                // Configuration of diagnosers and outputs
                //.AddDiagnoser(MemoryDiagnoser.Default)
            .AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(
                  maxDepth: int.MaxValue, // Controls the depth of method calls to disassemble
                  printSource: true,      // Show source code alongside assembly
                  exportCombinedDisassemblyReport: true // Output combined disassembly report
               )))

                .AddColumnProvider(DefaultColumnProviders.Instance)
                .AddLogger(ConsoleLogger.Default)
                .AddExporter(CsvExporter.Default)
                .AddExporter(HtmlExporter.Default)
                .AddAnalyser(GetAnalysers().ToArray());
        }

        /// <summary>
        /// Get analyser for the cutom configuration
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<IAnalyser> GetAnalysers()
        {
            yield return EnvironmentAnalyser.Default;
            yield return OutliersAnalyser.Default;
            yield return MinIterationTimeAnalyser.Default;
            yield return MultimodalDistributionAnalyzer.Default;
            yield return RuntimeErrorAnalyser.Default;
            yield return ZeroMeasurementAnalyser.Default;
            yield return BaselineCustomAnalyzer.Default;
        }
    }
}