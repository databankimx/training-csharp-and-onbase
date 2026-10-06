#nullable disable

using System.Text.Json.Serialization;

namespace EForms.TrainingNavigator.Models
{
    public class Location
    {
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("zipCode")]
        public string ZipCode { get; set; }
    }
}
