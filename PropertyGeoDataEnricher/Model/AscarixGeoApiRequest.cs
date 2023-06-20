namespace PropertyGeoDataEnricher.Model
{
    /// <summary>
    /// Represent request object for getting longitude and latitude value.
    /// </summary>
    public class AscarixGeoApiRequest
    {
        public string Locality { get; set; }

        public string Zip { get; set; }

        public string Street { get; set; }

        public int StreetNumber { get; set; }
    }
}
