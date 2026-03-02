using Measures.Application.DTOs;
using SharedKernel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// WASM-seitiger Maßnahmen-Service.
/// Kommuniziert per HTTP mit der Server-API.
/// JWT wird pro Request aus dem TokenService als Bearer-Token gesetzt.
/// </summary>
public sealed class MeasureApiService
{
    private readonly HttpClient   _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MeasureApiService(HttpClient http, TokenService tokenService)
    {
        _http         = http;
        _tokenService = tokenService;
    }

    public async Task<Result<List<MeasureSummaryDto>>> GetMeasuresAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var items = await _http.GetFromJsonAsync<List<MeasureSummaryDto>>("api/v1/measures", JsonOptions, ct);
            return Result.Success(items ?? []);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MeasureSummaryDto>>(new Error("Measure.LoadFailed", ex.Message));
        }
    }

    public async Task<Result<MeasureDto>> GetMeasureByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var item = await _http.GetFromJsonAsync<MeasureDto>($"api/v1/measures/{id}", JsonOptions, ct);
            if (item is null)
                return Result.Failure<MeasureDto>(new Error("General.NotFound", "Maßnahme nicht gefunden."));
            return Result.Success(item);
        }
        catch (Exception ex)
        {
            return Result.Failure<MeasureDto>(new Error("Measure.LoadFailed", ex.Message));
        }
    }

    public async Task<Result<MeasureDto>> CreateMeasureAsync(CreateMeasureDto dto, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/v1/measures", dto, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure<MeasureDto>(new Error(error.Code, error.Message));
            }

            var created = await response.Content.ReadFromJsonAsync<MeasureDto>(JsonOptions, ct);
            if (created is null)
                return Result.Failure<MeasureDto>(new Error("Measure.InvalidResponse", "Ungültige Serverantwort."));

            return Result.Success(created);
        }
        catch (Exception ex)
        {
            return Result.Failure<MeasureDto>(new Error("Measure.CreateFailed", ex.Message));
        }
    }

    public async Task<Result<MeasureDto>> UpdateMeasureAsync(Guid id, UpdateMeasureDto dto, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PutAsJsonAsync($"api/v1/measures/{id}", dto, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure<MeasureDto>(new Error(error.Code, error.Message));
            }

            var updated = await response.Content.ReadFromJsonAsync<MeasureDto>(JsonOptions, ct);
            if (updated is null)
                return Result.Failure<MeasureDto>(new Error("Measure.InvalidResponse", "Ungültige Serverantwort."));

            return Result.Success(updated);
        }
        catch (Exception ex)
        {
            return Result.Failure<MeasureDto>(new Error("Measure.UpdateFailed", ex.Message));
        }
    }

    public async Task<Result> DeleteMeasureAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/v1/measures/{id}", ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(error.Code, error.Message));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Measure.DeleteFailed", ex.Message));
        }
    }

    // ── Interne Hilfsmethoden ─────────────────────────────────────────────────

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
        catch
        {
            return ("Api.Error", $"HTTP {(int)response.StatusCode}");
        }
    }
}
