using System.Text.Json.Serialization;
using Shelly_UI.Models;

namespace Shelly_UI;

[JsonSerializable(typeof(ShellyConfig))]
[JsonSerializable(typeof(CachedRssModel))]
[JsonSerializable(typeof(RssModel))]
internal partial class ShellyUIJsonContext : JsonSerializerContext
{
}
