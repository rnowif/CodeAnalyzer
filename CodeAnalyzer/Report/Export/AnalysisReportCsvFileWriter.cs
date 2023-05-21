using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace CodeAnalyzer.Report.Export;

public class AnalysisReportCsvFileWriter
{
    private readonly AnalysisReport _report;

    public AnalysisReportCsvFileWriter(AnalysisReport report)
    {
        _report = report;
    }

    public Stream OpenStream()
    {
        var csvRecords = _report.ClassesReports.Select(p => p.Value);

        var memoryStream = new MemoryStream();
        TextWriter textWriter = new StreamWriter(memoryStream);
        var csv = new CsvWriter(textWriter, new CsvConfiguration(CultureInfo.InvariantCulture) {HasHeaderRecord = true});
        csv.WriteRecords(csvRecords);

        textWriter.Flush();
        memoryStream.Position = 0;

        return memoryStream;
    }
}
