using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CodeAnalyzer.Metrics;
using CsvHelper;
using CsvHelper.Configuration;

namespace CodeAnalyzer.Export
{
    public class AnalysisReportCsvFileWriter
    {
        private readonly AnalysisReport _report;

        public AnalysisReportCsvFileWriter(AnalysisReport report)
        {
            _report = report;
        }

        public Stream OpenStream()
        {
            IEnumerable<ClassAnalysisReport> csvRecords = _report.ClassesReports.Select(p => p.Value);

            var memoryStream = new MemoryStream();
            TextWriter textWriter = new StreamWriter(memoryStream);
            var csv = new CsvWriter(textWriter, new CsvConfiguration(CultureInfo.InvariantCulture) {HasHeaderRecord = true});
            csv.WriteRecords(csvRecords);

            textWriter.Flush();
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
