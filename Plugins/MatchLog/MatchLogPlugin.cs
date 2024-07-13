using AURankedPlugin.Models;
using AURankedPlugin.Plugins.MatchLog.EventListeners;
using AURankedPlugin.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using Impostor.Api.Events.Managers;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace AURankedPlugin.Plugins.MatchLog
{
    internal class MatchLogPlugin : IPluginHandler
    {

        public readonly ILogger<MatchLogPlugin> _logger;
        public readonly IEventManager _eventmanager;
        private IDisposable _unregister;
        private readonly double version = 1.1;
        public MatchLogConfig config;

        public MatchLogPlugin(ILogger<MatchLogPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventmanager = eventManager;
            config = LoadConfig();
        }

        public async ValueTask onEnableAsync()
        {

            string dataDirectory = Environment.CurrentDirectory;
            string seasonName = config.seasonName;

            string directoryPath = Path.Combine(dataDirectory, "plugins", "MatchLogs", seasonName);
            bool directoryExists = Directory.Exists(directoryPath);

            if (!directoryExists)
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, "matches.csv");
            if (!File.Exists(filePath))
            {
                CreateCSV(filePath, directoryPath);
            }

            _logger.LogInformation($"MatchLogPlugin has been loaded!");

            _unregister = _eventmanager.RegisterListener(new MatchLogListener(_logger, _eventmanager, config));
            await Task.CompletedTask;
        }

        public async ValueTask onDisableAsync()
        {
            _logger.LogInformation($"MatchLog has been unloaded");
            _unregister.Dispose();
            await Task.CompletedTask;
        }

        private void CreateCSV(string path, string directoryPath)
        {

            FileStream filestream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter writer = new StreamWriter(filestream);
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            CsvWriter csvWriter = new CsvWriter(writer, csvConfiguration);
            csvWriter.WriteHeader<Season>();
            csvWriter.NextRecord();

            var records = new List<Season>();
            var sorted_matches = RetrieveMatches(directoryPath).OrderBy(m => DateTime.Parse(m.gameStarted)).ToList();

            foreach (Match match in sorted_matches)
            {
                Season season = new Season();
                season.Id = match.MatchID;
                season.Match = match.eventsLogFile.Replace("_events.json", "_match.json");
                records.Add(season);
            }
            csvWriter.WriteRecords<Season>(records);
            writer.Close();
            filestream.Close();

        }

        public List<Match> RetrieveMatches(string directoryPath)
        {
            var matches = new List<Match>();
            foreach (string filename in Directory.GetFiles(directoryPath, "*_match.json"))
            {
                try
                {
                    matches.Add(JsonSerializer.Deserialize<Match>(File.ReadAllText(filename)));
                }
                catch (Exception e)
                {
                    _logger.LogError($"File: {filename}");
                    _logger.LogError(e.Message);
                }
            }
            return matches;
        }

        private MatchLogConfig LoadConfig()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "config");
            var filePath = Path.Combine(path, "logging.json");
            MatchLogConfig config;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            if (File.Exists(filePath))
            {
                config = JsonSerializer.Deserialize<MatchLogConfig>(File.ReadAllText(filePath));
            }
            else
            {
                config = new MatchLogConfig();
                File.WriteAllText(filePath, JsonSerializer.Serialize(config, options));
            }
            return config;

        }

    }
}
