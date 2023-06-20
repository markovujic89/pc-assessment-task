using Newtonsoft.Json;

namespace PropertyGeoDataEnricher.Model
{
    /// <summary>
    /// Represent response object for receving information for latitude and longitude values.
    /// </summary>
    public class AscarixGeoApiResponse
    {
        [JsonProperty("latitude")]
        public decimal Latitude { get; set; }

        [JsonProperty("longitude")]
        public decimal Longitude { get; set; }
    }
}
