using ImportExport.Contracts;
using SharedKernel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// WASM-seitiger Service für Import/Export-Endpunkte.
/// </summary>
public sealed class ImportExportApiService
{
    private readonly HttpClient   _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ImportExportApiService(HttpClient http, TokenService tokenService)
    {
        _http         = http;
        _tokenService = tokenService;
    }

    // ── Entity Registry ───────────────────────────────────────────────────────

    public async Task<Result<List<ExportableEntityDto>>> GetExportableEntitiesAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var items = await _http.GetFromJsonAsync<List<ExportableEntityDto>>("api/v1/import-export/entities", JsonOptions, ct);
            return Result.Success(items ?? []);
        }
        catch (Exception ex) { return Result.Failure<List<ExportableEntityDto>>(new Error("ImportExport.LoadEntitiesFailed", ex.Message)); }
    }

    // ── Channels ──────────────────────────────────────────────────────────────

    public async Task<Result<List<ChannelInfoDto>>> GetChannelsAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var items = await _http.GetFromJsonAsync<List<ChannelInfoDto>>("api/v1/import-export/channels", JsonOptions, ct);
            return Result.Success(items ?? []);
        }
        catch (Exception ex) { return Result.Failure<List<ChannelInfoDto>>(new Error("ImportExport.LoadChannelsFailed", ex.Message)); }
    }

    // ── Mapping Profiles ──────────────────────────────────────────────────────

    public async Task<Result<List<MappingProfileDto>>> GetMappingProfilesAsync(string? entityTypeName = null, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var url = entityTypeName is not null
                ? $"api/v1/import-export/mappings?entityTypeName={Uri.EscapeDataString(entityTypeName)}"
                : "api/v1/import-export/mappings";
            var items = await _http.GetFromJsonAsync<List<MappingProfileDto>>(url, JsonOptions, ct);
            return Result.Success(items ?? []);
        }
        catch (Exception ex) { return Result.Failure<List<MappingProfileDto>>(new Error("ImportExport.LoadMappingsFailed", ex.Message)); }
    }

    public async Task<Result<MappingProfileDto>> CreateMappingProfileAsync(CreateMappingProfileRequest request, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/v1/import-export/mappings", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure<MappingProfileDto>(new Error(err.Code, err.Message));
            }
            var created = await response.Content.ReadFromJsonAsync<MappingProfileDto>(JsonOptions, ct);
            return created is not null
                ? Result.Success(created)
                : Result.Failure<MappingProfileDto>(new Error("ImportExport.InvalidResponse", "Ungültige Serverantwort."));
        }
        catch (Exception ex) { return Result.Failure<MappingProfileDto>(new Error("ImportExport.CreateMappingFailed", ex.Message)); }
    }

    public async Task<Result<MappingProfileDto>> UpdateMappingProfileAsync(Guid id, UpdateMappingProfileRequest request, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PutAsJsonAsync($"api/v1/import-export/mappings/{id}", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure<MappingProfileDto>(new Error(err.Code, err.Message));
            }
            var updated = await response.Content.ReadFromJsonAsync<MappingProfileDto>(JsonOptions, ct);
            return updated is not null
                ? Result.Success(updated)
                : Result.Failure<MappingProfileDto>(new Error("ImportExport.InvalidResponse", "Ungültige Serverantwort."));
        }
        catch (Exception ex) { return Result.Failure<MappingProfileDto>(new Error("ImportExport.UpdateMappingFailed", ex.Message)); }
    }

    public async Task<Result> DeleteMappingProfileAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/v1/import-export/mappings/{id}", ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(err.Code, err.Message));
            }
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(new Error("ImportExport.DeleteMappingFailed", ex.Message)); }
    }

    // ── Export ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sendet eine Export-Anfrage und gibt die rohen Bytes + Content-Type + Dateiname zurück,
    /// damit der Browser einen Datei-Download starten kann.
    /// </summary>
    public async Task<Result<ExportResult>> ExportAsync(ExportRequest request, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/v1/import-export/export", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure<ExportResult>(new Error(err.Code, err.Message));
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName    = TryExtractFileName(response) ?? $"export.{(request.Channel == "Csv" ? "csv" : "xlsx")}";
            var data        = await response.Content.ReadAsByteArrayAsync(ct);
            return Result.Success(new ExportResult(fileName, contentType, data));
        }
        catch (Exception ex) { return Result.Failure<ExportResult>(new Error("ImportExport.ExportFailed", ex.Message)); }
    }

    // ── Import ────────────────────────────────────────────────────────────────

    public async Task<Result<ImportResult>> ImportAsync(
        string        entityTypeName,
        string        channel,
        Stream        fileStream,
        string        fileName,
        Guid?         mappingProfileId = null,
        CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(entityTypeName), "EntityTypeName");
            content.Add(new StringContent(channel),        "Channel");
            if (mappingProfileId.HasValue)
                content.Add(new StringContent(mappingProfileId.Value.ToString()), "MappingProfileId");

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(fileContent, "file", fileName);

            var response = await _http.PostAsync("api/v1/import-export/import", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure<ImportResult>(new Error(err.Code, err.Message));
            }

            var result = await response.Content.ReadFromJsonAsync<ImportResult>(JsonOptions, ct);
            return result is not null
                ? Result.Success(result)
                : Result.Failure<ImportResult>(new Error("ImportExport.InvalidResponse", "Ungültige Serverantwort."));
        }
        catch (Exception ex) { return Result.Failure<ImportResult>(new Error("ImportExport.ImportFailed", ex.Message)); }
    }

    // ── Intern ────────────────────────────────────────────────────────────────

    private async Task AttachTokenAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
            ? new AuthenticationHeaderValue("Bearer", token)
            : null;
    }

    private static async Task<(string Code, string Message)> TryReadApiErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var code    = doc.RootElement.TryGetProperty("error",   out var c) ? c.GetString() ?? "Api.Error" : "Api.Error";
            var message = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? response.ReasonPhrase ?? "Unbekannter Fehler" : response.ReasonPhrase ?? "Unbekannter Fehler";
            return (code, message);
        }
        catch { return ("Api.Error", $"HTTP {(int)response.StatusCode}"); }
    }

    private static string? TryExtractFileName(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentDisposition?.FileNameStar is { } star && !string.IsNullOrEmpty(star))
            return star;
        if (response.Content.Headers.ContentDisposition?.FileName is { } name && !string.IsNullOrEmpty(name))
            return name.Trim('"');
        return null;
    }

    private static string GetMimeType(string fileName)
        => Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv"  => "text/csv",
            _       => "application/octet-stream"
        };
}
