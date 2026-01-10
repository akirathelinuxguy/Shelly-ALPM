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
