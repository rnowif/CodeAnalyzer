using System.IO;
using System.Threading.Tasks;

namespace CodeAnalyzer.Report.Export;

public static class AnalysisReportExportExtensions
{
    public static async Task ExportToCsv(this AnalysisReport report, string filePath)
    {
        var writer = new AnalysisReportCsvFileWriter(report);
        await using var csvFile = File.Create(filePath);
        await writer.OpenStream().CopyToAsync(csvFile);
    }
}
