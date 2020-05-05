using cowrie_logviewer_data_analysis_tool.Runner;
using cowrie_logviewer_data_analysis_tool.Util;
using CsvHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static cowrie_logviewer_data_analysis_tool.Util.TextFileHandler;

namespace cowrie_logviewer_data_analysis_tool.Scripts
{
    class FormatCovid19Data : BaseScript, IReadWriteFiles
    {
        public override string ScriptName => "CovidCSVGen";

        public override string Description => "Uses covid csv data to map to appropiate ip data";

        public override string By => "mauh@itu.dk & milr@itu.dk 2020";

        public Dictionary<string, CountyCode> counties;

        public override void Run() {
            ReadFilesWriteToFile(GetSrcFolder("Data"), "owid-covid-data.csv", OutFile);
        }

        public override void Setup()
        {
            var InputFilePath = GetSrcPath("Data", "CountryMap.csv");

            counties = new Dictionary<string, CountyCode>();

            using (var reader = new StreamReader(InputFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();
                var records = csv.GetRecords<CountyCodeDTO>();
                records.ToList().ForEach(cc => counties.Add(cc.alpha_3, CountyCode.Parse(cc)));
            }
        }

        public CountyCode CountiesLookup(string data)
        {
            CountyCode info = null;
            counties.TryGetValue(data.ToUpper(), out info);
            return info;
        }

        public BlockingCollection<string> ReadFilesWriteToFile(string folder, string fileNameWC, string toFile)
        {
            var matchesCollection = new BlockingCollection<string>();

            var files = Directory.GetFiles(folder, fileNameWC,
                                         SearchOption.TopDirectoryOnly);

            Console.WriteLine("Reading Folder: " + folder);

            var readTask = Task.Run(() =>
            {
                using (var writer = new StreamWriter(toFile))
                {
                    //CSV Header
                    writer.WriteLine("date,countryCode,total_cases");
                    DateTime? currentHour = null;
                    Dictionary<DateTime, string[]> hours = new Dictionary<DateTime, string[]>();
                    try
                    {
                        foreach (var file in files)
                        {
                            string line2 = "";
                            string[] lines2 = null;
                            try
                            {
                                using (var reader = new StreamReader(file))
                                {
                                    string line;
                                    bool first = true;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (first) { first = false; continue; }
                                        line2 = line;

                                        string[] lines = line.Split(',');
                                        lines2 = lines;
                                        if (lines[0].Equals("")) continue;

                                        //Checks
                                        CovidDTO dto = new CovidDTO() { date = DateTime.ParseExact(lines[2], "yyyy-MM-dd", CultureInfo.InvariantCulture), iso_code = lines[0], total_cases = long.Parse(lines[3]) };
                                        if (dto.total_cases == 0) continue;
                                        var ctn = CountiesLookup(dto.iso_code);
                                        if (ctn == null) continue;

                                        var wholeHour = dto.date.Date;
                                        if (hours.ContainsKey(wholeHour))
                                        {

                                            hours.TryGetValue(wholeHour, out string[] dataFromHour);

                                            if (dataFromHour.ToList().Contains(ctn.country_code)) continue;
                                            else
                                            {
                                                var newips = dataFromHour.Append(ctn.country_code + "," + dto.total_cases);
                                                hours.Remove(wholeHour);
                                                hours.Add(wholeHour, newips.ToArray());
                                            }

                                        }
                                        else
                                        {

                                            if (currentHour != null)
                                            {
                                                var c_hour = currentHour.Value;
                                                hours.TryGetValue(c_hour, out string[] values);
                                                foreach (var value in values)
                                                {
                                                    writer.WriteLine(c_hour.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm"/*:ss"*/) + "," + value);
                                                }
                                            }

                                            currentHour = wholeHour;
                                            hours.Add(wholeHour, new string[] { ctn.country_code + "," + dto.total_cases });

                                        }
                                    }
                                }
                            }
                            catch (DirectoryNotFoundException e)
                            {
                                Console.WriteLine(e.StackTrace);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(line2);
                                Console.WriteLine(e.StackTrace);
                            }
                        }
                    }

                    finally
                    {
                        matchesCollection.CompleteAdding();
                    }
                }
            });

            Task.WaitAll(readTask);

            return matchesCollection;

        }
    
    }
}
