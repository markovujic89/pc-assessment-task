namespace PropertyGeoDataEnricher.Model
{
    public class GeoData
    {
        public string Zip { get; set; }

        public string Street { get; set; }

        public int StreetNumber { get; set; }

        public string Loclaity { get; set; }

        public Geometry Geometry { get; set; }
    }
}
