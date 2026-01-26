using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PackageManager.Aur.Models;

public class AurResponse<T>
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("resultcount")]
    public int ResultCount { get; set; }

    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = [];

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
