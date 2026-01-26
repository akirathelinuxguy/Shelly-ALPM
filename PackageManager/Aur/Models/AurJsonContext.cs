using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PackageManager.Aur.Models;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(AurResponse<AurPackageDto>))]
[JsonSerializable(typeof(AurUpdateDto))]
[JsonSerializable(typeof(List<string>))]
public partial class AurJsonContext : JsonSerializerContext
{
}