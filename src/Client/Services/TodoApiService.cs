using SharedKernel;
using Todos.Application.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// WASM-seitiger Todo-Service.
/// Kommuniziert per HTTP mit der Server-API. Kein MediatR.
/// JWT wird pro Request aus dem TokenService als Bearer-Token gesetzt.
/// </summary>
public sealed class TodoApiService
{
    private readonly HttpClient   _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TodoApiService(HttpClient http, TokenService tokenService)
    {
        _http         = http;
        _tokenService = tokenService;
    }

    public async Task<Result<List<TodoDto>>> GetUserTodosAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var todos = await _http.GetFromJsonAsync<List<TodoDto>>("api/v1/todos", JsonOptions, ct);
            return Result.Success(todos ?? []);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TodoDto>>(new Error("Todo.LoadFailed", ex.Message));
        }
    }

    public async Task<Result<TodoDto>> GetTodoByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var todo = await _http.GetFromJsonAsync<TodoDto>($"api/v1/todos/{id}", JsonOptions, ct);
            if (todo is null)
                return Result.Failure<TodoDto>(new Error("General.NotFound", "Todo nicht gefunden."));
            return Result.Success(todo);
        }
        catch (Exception ex)
        {
            return Result.Failure<TodoDto>(new Error("Todo.LoadFailed", ex.Message));
        }
    }

    public async Task<Result<List<TodoDto>>> GetOverdueTodosAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var todos = await _http.GetFromJsonAsync<List<TodoDto>>("api/v1/todos/overdue", JsonOptions, ct);
            return Result.Success(todos ?? []);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TodoDto>>(new Error("Todo.LoadFailed", ex.Message));
        }
    }

    public async Task<Result<TodoDto>> CreateTodoAsync(CreateTodoDto dto, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/v1/todos", dto, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure<TodoDto>(new Error(error.Code, error.Message));
            }

            var created = await response.Content.ReadFromJsonAsync<TodoDto>(JsonOptions, ct);
            if (created is null)
                return Result.Failure<TodoDto>(new Error("Todo.InvalidResponse", "Ungültige Serverantwort."));

            return Result.Success(created);
        }
        catch (Exception ex)
        {
            return Result.Failure<TodoDto>(new Error("Todo.CreateFailed", ex.Message));
        }
    }

    public async Task<Result<TodoDto>> UpdateTodoAsync(string id, UpdateTodoDto dto, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PutAsJsonAsync($"api/v1/todos/{id}", dto, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure<TodoDto>(new Error(error.Code, error.Message));
            }

            var updated = await response.Content.ReadFromJsonAsync<TodoDto>(JsonOptions, ct);
            if (updated is null)
                return Result.Failure<TodoDto>(new Error("Todo.InvalidResponse", "Ungültige Serverantwort."));

            return Result.Success(updated);
        }
        catch (Exception ex)
        {
            return Result.Failure<TodoDto>(new Error("Todo.UpdateFailed", ex.Message));
        }
    }

    /// <summary>
    /// Ändert den Status eines Todos.
    /// Mappt auf die spezifischen API-Endpunkte (complete/start/cancel).
    /// </summary>
    public async Task<Result> UpdateTodoStatusAsync(string id, TodoStatus status, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();

            var endpoint = status switch
            {
                TodoStatus.Completed  => $"api/v1/todos/{id}/complete",
                TodoStatus.InProgress => $"api/v1/todos/{id}/start",
                TodoStatus.Cancelled  => $"api/v1/todos/{id}/cancel",
                _                     => null
            };

            if (endpoint is null)
                return Result.Failure(new Error("Todo.InvalidStatus", $"Status '{status}' kann nicht direkt gesetzt werden."));

            var response = await _http.PostAsync(endpoint, null, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(error.Code, error.Message));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Todo.StatusUpdateFailed", ex.Message));
        }
    }

    public async Task<Result> DeleteTodoAsync(string id, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/v1/todos/{id}", ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(error.Code, error.Message));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Todo.DeleteFailed", ex.Message));
        }
    }

    // ── Interne Hilfsmethoden ─────────────────────────────────────────────────

    private async Task AttachTokenAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    private static async Task<(string Code, string Message)> TryReadApiErrorAsync(
        HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var code    = doc.RootElement.TryGetProperty("error",   out var c)
                        ? c.GetString() ?? "Api.Error"
                        : "Api.Error";
            var message = doc.RootElement.TryGetProperty("message", out var m)
                        ? m.GetString() ?? response.ReasonPhrase ?? "Unbekannter Fehler"
                        : response.ReasonPhrase ?? "Unbekannter Fehler";
            return (code, message);
        }
        catch
        {
            return ("Api.Error", $"HTTP {(int)response.StatusCode}");
        }
    }
}
