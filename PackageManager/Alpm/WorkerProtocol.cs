using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PackageManager.Alpm;

public class WorkerRequest
{
    public string Command { get; set; } = string.Empty;
    public string? Payload { get; set; }
}

public class WorkerResponse
{
    public bool Success { get; set; }
    public string? Data { get; set; }
    public string? Error { get; set; }
}

[JsonSerializable(typeof(WorkerRequest))]
[JsonSerializable(typeof(WorkerResponse))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<AlpmPackageDto>))]
[JsonSerializable(typeof(List<AlpmPackageUpdateDto>))]
public partial class AlpmWorkerJsonContext : JsonSerializerContext
{
}
