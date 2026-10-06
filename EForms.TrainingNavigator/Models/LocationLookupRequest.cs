#nullable enable

using System.Text.Json.Serialization;

namespace EForms.TrainingNavigator.Models
{
    public class LocationLookupRequest
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; }
    }
}
