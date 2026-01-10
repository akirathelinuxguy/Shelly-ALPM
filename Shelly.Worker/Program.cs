using System.Text.Json;
using PackageManager.Alpm;

namespace Shelly.Worker;

class Program
{
    static void Main(string[] args)
    {
        using var manager = new AlpmManager();

        while (true)
        {
            var line = Console.ReadLine();
            if (line == null) break;

            WorkerRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<WorkerRequest>(line);
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
                        manager.Initialize();
                        var available = manager.GetAvailablePackages();
                        response.Data = JsonSerializer.Serialize(available);
                        break;

                    case "GetInstalledPackages":
                        manager.Initialize();
                        var installed = manager.GetInstalledPackages();
                        response.Data = JsonSerializer.Serialize(installed);
                        break;

                    case "GetPackagesNeedingUpdate":
                        manager.IntializeWithSync();
                        var updates = manager.GetPackagesNeedingUpdate();
                        response.Data = JsonSerializer.Serialize(updates);
                        break;

                    case "Sync":
                        manager.IntializeWithSync();
                        break;

                    case "InstallPackages":
                        if (request.Payload == null) throw new Exception("Missing packages list");
                        var packagesToInstall = JsonSerializer.Deserialize<List<string>>(request.Payload);
                        manager.Initialize();
                        manager.InstallPackages(packagesToInstall!);
                        break;

                    case "UpdatePackages":
                        if (request.Payload == null) throw new Exception("Missing packages list");
                        var packagesToUpdate = JsonSerializer.Deserialize<List<string>>(request.Payload);
                        manager.Initialize();
                        manager.UpdatePackages(packagesToUpdate!);
                        break;

                    case "RemovePackage":
                        if (request.Payload == null) throw new Exception("Missing package name");
                        manager.Initialize();
                        manager.RemovePackage(request.Payload);
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

            Console.WriteLine(JsonSerializer.Serialize(response));
        }
    }
}
