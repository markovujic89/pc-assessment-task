using MethodTimer;
using Newtonsoft.Json;
using PropertyGeoDataEnricher.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGeoDataEnricher
{
    public class DataProvider : IDataProvider
    {
        private HttpClient httpClient;

        private const string requestUrl = "http://localhost:5000/api/geo";
        public DataProvider()
        {
            httpClient = new HttpClient();
        }

        [Time]
        public ConcurrentDictionary<string, CsvData> ParseCsvFile(string filePath, int numberOfThreads)
        {
            var csvDataMap = new ConcurrentDictionary<string, CsvData>();
            var linesQueue = new ConcurrentQueue<string>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        linesQueue.Enqueue(line);
                    }
                }

                Task[] tasks = new Task[numberOfThreads];
                for (int i = 0; i < numberOfThreads; i++)
                {
                    tasks[i] = Task.Run(() => ProcessLines(csvDataMap, linesQueue));
                }

                Task.WaitAll(tasks);

                return csvDataMap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during parsing input Csv file: {ex.Message}");
                return csvDataMap;
            }

        }

        [Time]
        public async Task<List<GeoData>> GetGeoDataAsync(ConcurrentDictionary<string, CsvData> csvDataMap)
        {
            var geoDataCollection = new ConcurrentBag<GeoData>();
            var tasks = new List<Task>();

            foreach (var value in csvDataMap.Values)
            {
                if (!string.IsNullOrEmpty(value.Street))
                {
                    var requestBody = new AscarixGeoApiRequest
                    {
                        Locality = value.Loclaity,
                        Zip = value.Zip,
                        Street = value.Street,
                        StreetNumber = value.StreetNumber
                    };

                    var jsonBody = JsonConvert.SerializeObject(requestBody);
                    var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var response = await httpClient.PostAsync(requestUrl, httpContent);
                            response.EnsureSuccessStatusCode();

                            var responseBody = await response.Content.ReadAsStringAsync();
                            var responseObject = JsonConvert.DeserializeObject<AscarixGeoApiResponse>(responseBody);

                            geoDataCollection.Add(new GeoData
                            {
                                Loclaity = value.Loclaity,
                                Zip = value.Zip,
                                Street = value.Street,
                                StreetNumber = value.StreetNumber,
                                Geometry = new Geometry { Latitude = responseObject.Latitude, Longitude = responseObject.Longitude }
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception occured during calling AscarixGeoApiResponse {ex}");
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);

            return geoDataCollection.ToList();
        }

        private void ProcessLines(ConcurrentDictionary<string, CsvData> csvDataMap, ConcurrentQueue<string> linesQueue)
        {
            while (linesQueue.TryDequeue(out string line))
            {
                string[] values = line.Split(';');

                // I took a lot of time to figure out how to parse a csv file and obtain all the necessary information for getting longitude and latitude values.
                // From NEW_PLZ1, I took information about ZipCode and Locality; from NEW_STR, I took street name and streetId (connect with NEW_PLZ1 via ONRP); and from NEW_GEB, I took information about
                // about street number (connect with NEW_STR via STRID)
                // Hope that is a righy way :D

                // REC_ART = 01 for NEW_PLZ1
                if (values[0] == "01")
                {
                    csvDataMap.GetOrAdd(values[1], value =>
                        new CsvData { Zip = values[4], Loclaity = values[7] });
                }
                // REC_ART = 04 for NEW_STR
                else if (values[0] == "04")
                {
                    if (csvDataMap.TryGetValue(values[2], out CsvData output))
                    {
                        output.Street = values[4];
                        output.StreetId = values[1];
                    }
                }
                // REC_ART = 06 NEW_GEB
                else if (values[0] == "06")
                {
                    if (csvDataMap.TryGetValue(values[2], out CsvData output))
                    {
                        if (int.TryParse(values[3], out int streetNumber))
                        {
                            output.StreetNumber = streetNumber;
                        }
                    }
                }
            }
        }
    }
}
