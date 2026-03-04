using ClosedXML.Excel;
using ImportExport.Application.Channels;

namespace ImportExport.Infrastructure.Channels;

/// <summary>Excel-Channel via ClosedXML.</summary>
public sealed class ExcelImportChannel : IImportChannel
{
    public string Key          => "Excel";
    public string DisplayName  => "Excel (.xlsx)";
    public IReadOnlyList<string> AcceptedMimeTypes =>
    [
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel"
    ];

    public Task<IReadOnlyList<ImportRow>> ParseAsync(Stream stream, CancellationToken ct = default)
    {
        var rows = new List<ImportRow>();

        using var workbook  = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var usedRange = worksheet.RangeUsed();

        if (usedRange is null)
            return Task.FromResult<IReadOnlyList<ImportRow>>(rows);

        var headerRow = usedRange.FirstRow();
        var headers   = headerRow.Cells()
            .Select(c => c.GetValue<string>().Trim())
            .ToList();

        foreach (var row in usedRange.RowsUsed().Skip(1))
        {
            var dict = new Dictionary<string, string?>();
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = row.Cell(i + 1);
                dict[headers[i]] = cell.IsEmpty() ? null : cell.GetValue<string>();
            }
            rows.Add(new ImportRow(dict));
        }

        return Task.FromResult<IReadOnlyList<ImportRow>>(rows);
    }
}

public sealed class ExcelExportChannel : IExportChannel
{
    public string Key           => "Excel";
    public string DisplayName   => "Excel (.xlsx)";
    public string FileExtension => ".xlsx";
    public string ContentType   => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public Task<byte[]> WriteAsync(
        IEnumerable<string>                              headers,
        IEnumerable<IReadOnlyDictionary<string, string?>> rows,
        CancellationToken ct = default)
    {
        var headerList = headers.ToList();

        using var workbook  = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Export");

        // Header-Zeile
        for (int i = 0; i < headerList.Count; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headerList[i];
            cell.Style.Font.Bold = true;
        }

        // Datenzeilen
        int rowIndex = 2;
        foreach (var row in rows)
        {
            for (int i = 0; i < headerList.Count; i++)
            {
                var col = headerList[i];
                worksheet.Cell(rowIndex, i + 1).Value = row.TryGetValue(col, out var val) ? val ?? "" : "";
            }
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return Task.FromResult(ms.ToArray());
    }
}
