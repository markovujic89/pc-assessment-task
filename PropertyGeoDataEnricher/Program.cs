using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PropertyGeoDataEnricher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IDataProvider dataProvider = new DataProvider();

            // Pars CSV input file
            // 
            var csvMap = dataProvider.ParseCsvFile(@"D:\PropertyCaptainTask\PropertyGeoDataEnricher\PropertyGeoDataEnricher\Post_Adressdaten20170425.csv", 5);

            // Preper data for serilize to geoJson format
            var geoData = await dataProvider.GetGeoDataAsync(csvMap);

            var geoJsonString = JsonConvert.SerializeObject(geoData, Formatting.Indented);

            File.WriteAllText(@"C:\temp\output.geojson", geoJsonString);

            Console.WriteLine("GeoJSON file successfully created.");
        }
    }
}
