using SharedKernel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Taxonomy.Application.DTOs;

namespace App.Services;

/// <summary>
/// WASM-seitiger Taxonomy-Service (Categories + Tags).
/// JWT wird pro Request aus dem TokenService als Bearer-Token gesetzt.
/// </summary>
public sealed class TaxonomyApiService
{
    private readonly HttpClient   _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TaxonomyApiService(HttpClient http, TokenService tokenService)
    {
        _http         = http;
        _tokenService = tokenService;
    }

    // ── Categories ────────────────────────────────────────────────────────────

    public async Task<Result<List<CategoryDto>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var items = await _http.GetFromJsonAsync<List<CategoryDto>>("api/v1/categories", JsonOptions, ct);
            return Result.Success(items ?? []);
        }
        catch (Exception ex) { return Result.Failure<List<CategoryDto>>(new Error("Category.LoadFailed", ex.Message)); }
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/v1/categories", dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure<CategoryDto>(new Error(err.Code, err.Message));
            }
            var created = await response.Content.ReadFromJsonAsync<CategoryDto>(JsonOptions, ct);
            return created is not null ? Result.Success(created) : Result.Failure<CategoryDto>(new Error("Category.InvalidResponse", "Ungültige Serverantwort."));
        }
        catch (Exception ex) { return Result.Failure<CategoryDto>(new Error("Category.CreateFailed", ex.Message)); }
    }

    public async Task<Result> DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/v1/categories/{id}", ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(err.Code, err.Message));
            }
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(new Error("Category.DeleteFailed", ex.Message)); }
    }

    public Task<Result<CategoryDto>> CreateCategoryAsync(string label, CancellationToken ct = default)
        => CreateCategoryAsync(new CreateCategoryDto { Label = label }, ct);

    // ── Tags ──────────────────────────────────────────────────────────────────

    public async Task<Result<List<TagDto>>> GetTagsAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var items = await _http.GetFromJsonAsync<List<TagDto>>("api/v1/tags", JsonOptions, ct);
            return Result.Success(items ?? []);
        }
        catch (Exception ex) { return Result.Failure<List<TagDto>>(new Error("Tag.LoadFailed", ex.Message)); }
    }

    public async Task<Result<TagDto>> CreateTagAsync(CreateTagDto dto, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/v1/tags", dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure<TagDto>(new Error(err.Code, err.Message));
            }
            var created = await response.Content.ReadFromJsonAsync<TagDto>(JsonOptions, ct);
            return created is not null ? Result.Success(created) : Result.Failure<TagDto>(new Error("Tag.InvalidResponse", "Ungültige Serverantwort."));
        }
        catch (Exception ex) { return Result.Failure<TagDto>(new Error("Tag.CreateFailed", ex.Message)); }
    }

    public Task<Result<TagDto>> CreateTagAsync(string label, CancellationToken ct = default)
        => CreateTagAsync(new CreateTagDto { Label = label }, ct);

    public async Task<Result> DeleteTagAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/v1/tags/{id}", ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(err.Code, err.Message));
            }
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(new Error("Tag.DeleteFailed", ex.Message)); }
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
}
