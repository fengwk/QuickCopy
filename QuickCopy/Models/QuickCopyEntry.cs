using System.Linq;
using System.Text.Json.Serialization;

namespace QuickCopy.Models;

internal sealed class QuickCopyEntry
{
    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("shell")]
    public string? Shell { get; set; }

    [JsonIgnore]
    public string Label =>
        FirstNonEmpty(Display, Content, Shell) ?? "<empty>";

    [JsonIgnore]
    public bool HasShell => !string.IsNullOrWhiteSpace(Shell);

    [JsonIgnore]
    public string Subtitle => HasShell
        ? "Run shell and copy result to clipboard"
        : "Copy to clipboard";

    [JsonIgnore]
    public string SearchText => string.Join(
        " ",
        new[] { Display, Content, Shell }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim()));

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
