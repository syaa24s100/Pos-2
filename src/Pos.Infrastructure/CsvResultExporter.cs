using System.Globalization;
using System.Text;
using Pos.Application.Dtos;
using Pos.Application.Ports;

namespace Pos.Infrastructure;

public sealed class CsvResultExporter : IResultExporter
{
    public void Export(IEnumerable<ResultDto> results, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Студент;Итог, %;Итог (10-балльная);Оценено критериев;Всего критериев;Правило");
        foreach (var r in results)
        {
            sb.AppendLine(string.Join(';',
                Escape(r.StudentName),
                r.Percent.ToString("0.00", CultureInfo.InvariantCulture),
                r.Mark10.ToString("0.0", CultureInfo.InvariantCulture),
                r.RatedCriteria,
                r.TotalCriteria,
                r.Strategy));
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    private static string Escape(string value) =>
        value.Contains(';') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
}
