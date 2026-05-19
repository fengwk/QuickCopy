using System.Text.Json.Serialization;

namespace QuickCopy.Models;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(QuickCopyEntry[]))]
internal sealed partial class QuickCopyJsonSerializerContext : JsonSerializerContext
{
}
