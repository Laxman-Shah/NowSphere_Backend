
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace smartApi.ExceptionHandling.Models;

public class ErrorProblemDetails : ProblemDetails
{
    [JsonPropertyName("code")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("errors")]
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
}

