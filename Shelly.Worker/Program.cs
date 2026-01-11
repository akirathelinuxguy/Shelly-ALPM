using System.Text.Json;
using System.Text.Json.Serialization;
using PackageManager.Alpm;

namespace Shelly.Worker;

[JsonSerializable(typeof(WorkerRequest))]
[JsonSerializable(typeof(WorkerResponse))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<AlpmPackageDto>))]
[JsonSerializable(typeof(List<AlpmPackageUpdateDto>))]
internal partial class WorkerJsonContext : JsonSerializerContext
{
}

class Program
{
    static void Main(string[] args)
    {
        using var manager = new AlpmManager();
        manager.Initialize();

        while (true)
        {
            var line = Console.ReadLine();
            if (line == null) break;

            WorkerRequest? request;
            try
            {
                request = JsonSerializer.Deserialize(line, WorkerJsonContext.Default.WorkerRequest);
            }
            catch
            {
                continue;
            }

            if (request == null) continue;

            var response = new WorkerResponse { Success = true };

            try
            {
                switch (request.Command)
                {
                    case "GetAvailablePackages":
                        //manager.Initialize();
                        var available = manager.GetAvailablePackages();
                        response.Data = JsonSerializer.Serialize(available, WorkerJsonContext.Default.ListAlpmPackageDto);
                        break;

                    case "GetInstalledPackages":
                        //manager.Initialize();
                        var installed = manager.GetInstalledPackages();
                        response.Data = JsonSerializer.Serialize(installed, WorkerJsonContext.Default.ListAlpmPackageDto);
                        break;

                    case "GetPackagesNeedingUpdate":
                        manager.Sync();
                        var updates = manager.GetPackagesNeedingUpdate();
                        response.Data = JsonSerializer.Serialize(updates, WorkerJsonContext.Default.ListAlpmPackageUpdateDto);
                        break;

                    case "Sync":
                        manager.Sync();
                        break;

                    case "InstallPackages":
                        if (request.Payload == null) throw new Exception("Missing packages list");
                        var packagesToInstall = JsonSerializer.Deserialize(request.Payload, WorkerJsonContext.Default.ListString);
                        manager.InstallPackages(packagesToInstall!);
                        break;

                    case "UpdatePackages":
                        if (request.Payload == null) throw new Exception("Missing packages list");
                        var packagesToUpdate = JsonSerializer.Deserialize(request.Payload, WorkerJsonContext.Default.ListString);
                        manager.UpdatePackages(packagesToUpdate!);
                        break;

                    case "RemovePackage":
                        if (request.Payload == null) throw new Exception("Missing package name");
                        manager.RemovePackage(request.Payload);
                        break;
                    case "RemovePackages":
                        if (request.Payload == null) throw new Exception("Missing packages list");
                        var packagesToRemove = JsonSerializer.Deserialize(request.Payload, WorkerJsonContext.Default.ListString);
                        manager.RemovePackages(packagesToRemove!);
                        break;
                    case "SyncSystemUpdate":
                        manager.SyncSystemUpdate();
                        break;
                    case "Exit":
                        return;

                    default:
                        response.Success = false;
                        response.Error = $"Unknown command: {request.Command}";
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }

            Console.WriteLine(JsonSerializer.Serialize(response, WorkerJsonContext.Default.WorkerResponse));
        }
    }
}