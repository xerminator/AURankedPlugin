using AURankedPlugin.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AURankedPlugin.Utils
{
    public class FileUtils
    {

        public static int RetrieveMatchIDFromFile(string path)
        {
            string csvFilePath = Path.Combine(path, "matches.csv");
            var matchId = 0;
            if (File.Exists(csvFilePath))
            {
                var file = File.Open(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader streamReader = new StreamReader(file);
                CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                var records = csvReader.GetRecords<Season>().ToList();

                if (records.Count > 0)
                {
                    matchId = records.Max(r => r.Id);
                }

                streamReader.Close();
                file.Close();
            }
            return matchId;
        }

    }
}
