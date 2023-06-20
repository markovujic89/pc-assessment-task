using PropertyGeoDataEnricher.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PropertyGeoDataEnricher
{
    /// <summary>
    /// Data provider for obtaining data necessary for creating an output file in GeoJSON format.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Parse CSV input CSV file
        /// </summary>
        /// <param name="filePath">CSV file path.</param>
        /// <param name="numberOfThreads">The number of threads included in the parsing process.</param>
        /// <returns>Concurrent collection with key-value pairs.</returns>
        ConcurrentDictionary<string, CsvData> ParseCsvFile(string filePath, int numberOfThreads);

        /// <summary>
        /// Create collection of GeoData which will be serialize in geojson format
        /// </summary>
        /// <param name="mapCsvData">Parsed data from csv input file.</param>
        /// <returns>Collection of prepered data for geojson output file.</returns>
        Task<List<GeoData>> GetGeoDataAsync(ConcurrentDictionary<string, CsvData> csvDataMap);
    }
}
