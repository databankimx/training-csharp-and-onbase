#nullable enable

using System.Text.Json.Serialization;

namespace EForms.TrainingNavigator.Models
{
    public class LocationLookupResponse
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("data")]
        public List<Location> Data { get; set; } = new();

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();
    }
}
