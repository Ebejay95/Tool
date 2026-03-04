using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ImportExport.Application.Channels;

namespace ImportExport.Infrastructure.Channels;

/// <summary>CSV-Channel via CsvHelper.</summary>
public sealed class CsvImportChannel : IImportChannel
{
    public string Key          => "Csv";
    public string DisplayName  => "CSV (.csv)";
    public IReadOnlyList<string> AcceptedMimeTypes => ["text/csv", "application/csv"];

    public async Task<IReadOnlyList<ImportRow>> ParseAsync(Stream stream, CancellationToken ct = default)
    {
        var rows   = new List<ImportRow>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord  = true,
            MissingFieldFound = null,
            BadDataFound      = null,
        };

        using var reader    = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var csvReader = new CsvReader(reader, config);

        await csvReader.ReadAsync();
        csvReader.ReadHeader();
        var headers = csvReader.HeaderRecord ?? [];

        while (await csvReader.ReadAsync())
        {
            var dict = new Dictionary<string, string?>();
            foreach (var header in headers)
                dict[header] = csvReader.TryGetField<string>(header, out var val) ? val : null;

            rows.Add(new ImportRow(dict));
        }

        return rows;
    }
}

public sealed class CsvExportChannel : IExportChannel
{
    public string Key           => "Csv";
    public string DisplayName   => "CSV (.csv)";
    public string FileExtension => ".csv";
    public string ContentType   => "text/csv";

    public Task<byte[]> WriteAsync(
        IEnumerable<string>                              headers,
        IEnumerable<IReadOnlyDictionary<string, string?>> rows,
        CancellationToken ct = default)
    {
        var headerList = headers.ToList();
        var sb         = new StringBuilder();
        var config     = new CsvConfiguration(CultureInfo.InvariantCulture);

        using var writer    = new StringWriter(sb);
        using var csvWriter = new CsvWriter(writer, config);

        // Header
        foreach (var h in headerList)
            csvWriter.WriteField(h);
        csvWriter.NextRecord();

        // Zeilen
        foreach (var row in rows)
        {
            foreach (var h in headerList)
                csvWriter.WriteField(row.TryGetValue(h, out var val) ? val : null);
            csvWriter.NextRecord();
        }

        // UTF-8 BOM für Excel-Kompatibilität
        var bom   = Encoding.UTF8.GetPreamble();
        var data  = Encoding.UTF8.GetBytes(sb.ToString());
        var bytes = bom.Concat(data).ToArray();

        return Task.FromResult(bytes);
    }
}
