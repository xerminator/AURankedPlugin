using AURankedPlugin.Models;
using AURankedPlugin.Utils;
using CsvHelper;
using Impostor.Api.Events;
using Impostor.Api.Events.Managers;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using Match = AURankedPlugin.Models.Match;

namespace AURankedPlugin.Plugins.MatchLog.EventListeners
{
    internal class MatchLogListener : IEventListener
    {
        private readonly ILogger<MatchLogPlugin> _logger;
        private IEventManager _eventManager;
        //private Dictionary<GameCode, GameData> gameDataMap = new();
        private MatchLogConfig _config;
        public MatchLogListener(ILogger<MatchLogPlugin> logger, IEventManager eventManager, MatchLogConfig config)
        {
            _logger = logger;
            _eventManager = eventManager;
            _config = config;
        }

        [EventListener]
        public void onGameStarted(IGameStartedEvent e)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "plugins", "MatchLogs", _config.seasonName);
            int Id = FileUtils.RetrieveMatchIDFromFile(path) + 1;
            var gameData = new GameData(_logger, e.Game.Code, Id);
            foreach (var player in e.Game.Players)
            {
                if (player.Character == null) return;
                gameData.AddPlayer(player);
            }
            gameData.eventsLog.gameStartTimeSet();
            GameDataUtils.gameDataMap.Add(e.Game.Code, gameData);

            createMatchLog(e.Game.Code);
        }

        public void createMatchLog(GameCode gameCode)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(gameCode)) return;
            var game = GameDataUtils.gameDataMap[gameCode];
            string path = Path.Combine(Environment.CurrentDirectory, "plugins", "MatchLogs", _config.seasonName);

            string _players = string.Join(",", game.Players.Select(p => p.Character.PlayerInfo.PlayerName));
            string _impostors = string.Join(",", game.Impostors.Select(p => p.Character.PlayerInfo.PlayerName));

            string matchFilePath = Path.Combine(path, $"{game.matchId}_match.json");
            string eventsFilePath = Path.Combine(path, $"{game.matchId}_events.json");

            var eventsData = game.jsonifyRawData();

            var match = new Match
            {
                MatchID = game.matchId,
                gameStarted = game.eventsLog.gameStartedUTC.ToString(),
                players = _players,
                impostors = _impostors,
                eventsLogFile = $"{game.matchId}_events.json",
                result = "Unknown",
                reason = "Pending"
            };

            var matchjson = JsonSerializer.Serialize(match);
            File.WriteAllText(matchFilePath, matchjson);
            File.WriteAllText(eventsFilePath, eventsData);
            WriteToCsv(gameCode, path, $"{game.matchId}_match.json");
        }



        [EventListener(EventPriority.Monitor)]
        public void onGameEnd(IGameEndedEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;
            _logger.LogInformation("Game Ended, overwriting files");
            if (e.Game == null) return;
            GameCode gamecode = e.Game.Code;

            //get all data
            var gameData = GameDataUtils.gameDataMap[gamecode];
            var dbEntry = gameData.stringifyData();
            var rawData = gameData.jsonifyRawData();
            var resultString = gameData.canceled ? "Canceled" : getResult(e.GameOverReason);

            //Write logfile to directory
            string workingDirectory = Environment.CurrentDirectory;
            string directoryPath = Path.Combine(workingDirectory, "plugins", "MatchLogs", _config.seasonName);
            string filePath = Path.Combine(directoryPath, $"{gameData.matchId}_events.json");
            File.WriteAllText(filePath, rawData);

            var newMatch = new Match
            {
                MatchID = gameData.matchId,
                gameStarted = dbEntry[0],
                players = dbEntry[1],
                impostors = dbEntry[2],
                eventsLogFile = $"{gameData.matchId}_events.json",
                result = resultString,
                reason = e.GameOverReason.ToString()
            };

            string matchJson = JsonSerializer.Serialize(newMatch);
            string matchFilePath = Path.Combine(directoryPath, $"{gameData.matchId}_match.json");
            File.WriteAllText(matchFilePath, matchJson);

            GameDataUtils.gameDataMap[gamecode].Reset();
            GameDataUtils.gameDataMap.Remove(gamecode);


        }

        [EventListener]
        public void onExile(IMeetingEndedEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;
            if (e.Exiled != null)
            {
                GameDataUtils.gameDataMap[e.Game.Code].eventsLog.onExile(e.Exiled.PlayerInfo.PlayerName, DateTime.Now);
            }
        }

        public string getResult(GameOverReason reason)
        {
            List<GameOverReason> crew = new List<GameOverReason> { GameOverReason.HumansByVote, GameOverReason.HumansByTask };
            List<GameOverReason> imp = new List<GameOverReason> { GameOverReason.ImpostorByKill, GameOverReason.ImpostorBySabotage, GameOverReason.ImpostorByVote };

            string result = "Unknown";
            if (crew.Contains(reason))
            {
                result = "Crewmates Win";
            }
            else if (imp.Contains(reason))
            {
                result = "Impostors Win";
            }
            return result;
        }

        [EventListener]
        public void onReport(IPlayerStartMeetingEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;

            GameDataUtils.gameDataMap[e.Game.Code].inMeeting = true;
            bool bodyreport = false;
            if (e.ClientPlayer == null)
            {
                //Should never happen
                return;
            }
            if (e.Body != null)
            {
                bodyreport = true;
            }
            if (bodyreport)
            {
                if (e.ClientPlayer.Character != null && e.Body != null)
                    GameDataUtils.gameDataMap[e.Game.Code].eventsLog.onReport(e.ClientPlayer.Character.PlayerInfo.PlayerName, e.Body.PlayerInfo.PlayerName, DateTime.Now);
            }
            else if (e.ClientPlayer.Character != null)
            {
                GameDataUtils.gameDataMap[e.Game.Code].eventsLog.startMeeting(e.ClientPlayer.Character.PlayerInfo.PlayerName, DateTime.Now);
            }
            else
            {
                //Error
                return;
            }
        }

        [EventListener]
        public void onMeetingEnd(IMeetingEndedEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;

            string meetingResult = "Skipped";
            if (e.IsTie)
            {
                meetingResult = "Tie";
            }
            if (e.Exiled != null)
            {
                meetingResult = "Exiled";
            }
            GameDataUtils.gameDataMap[e.Game.Code].inMeeting = false;
            GameDataUtils.gameDataMap[e.Game.Code].eventsLog.endMeeting(DateTime.Now, meetingResult);
        }

        [EventListener]
        public async void onPlayerChat(IPlayerChatEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;
            if (e.Game.GameState != GameStates.Started) return;
            if (e.Message.ToLower().Trim().Equals("/cancel") || e.Message.ToLower().Trim().Equals("?cancel"))
            {
                e.IsCancelled = true;
                if (!e.ClientPlayer.IsHost) return;
                GameDataUtils.gameDataMap[e.Game.Code].canceled = true;
                GameDataUtils.gameDataMap[e.Game.Code].eventsLog.onCancel(e.PlayerControl.PlayerInfo.PlayerName, DateTime.Now);
                await e.PlayerControl.SendChatToPlayerAsync("Game has been logged as a cancel, type /end to end the game");
            }
            if (e.Message.ToLower().Trim().Equals("/end") || e.Message.ToLower().Trim().Equals("?end"))
            {
                e.IsCancelled = true;
                if (!GameDataUtils.gameDataMap[e.Game.Code].canceled) return;
                GameDataUtils.gameDataMap[e.Game.Code].eventsLog.onEnd(e.ClientPlayer, DateTime.Now);

                if (GameDataUtils.gameDataMap[e.Game.Code].Impostors.Count > 0)
                {
                    foreach (var player in GameDataUtils.gameDataMap[e.Game.Code].Players)
                    {
                        if (player.Character == null) return;

                        if (!player.Character.PlayerInfo.IsDead && GameDataUtils.gameDataMap[e.Game.Code].Impostors.Contains(player))
                        {

                            if (GameDataUtils.gameDataMap[e.Game.Code].Impostors[0].Character.PlayerInfo.IsDead && GameDataUtils.gameDataMap[e.Game.Code].Impostors.Count > 1)
                            {
                                var temp = GameDataUtils.gameDataMap[e.Game.Code].Impostors[0];
                                GameDataUtils.gameDataMap[e.Game.Code].Impostors[0] = GameDataUtils.gameDataMap[e.Game.Code].Impostors[1];
                                GameDataUtils.gameDataMap[e.Game.Code].Impostors[1] = temp;
                            }

                            if (GameDataUtils.gameDataMap[e.Game.Code].Impostors[0].Character != null)
                            {
                                await GameDataUtils.gameDataMap[e.Game.Code].Impostors[0].Character.MurderPlayerAsync(player.Character);
                            }

                        }
                    }
                }
            }

            else
            {
                return;
            }
        }

        [EventListener]
        public void onVote(IPlayerVotedEvent e)
        {

            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;

            string voted = "none";
            string player = "none";
            if (e.VotedFor != null)
            {
                voted = e.VotedFor.PlayerInfo.PlayerName;
            }
            if (e.ClientPlayer.Character != null)
            {
                player = e.ClientPlayer.Character.PlayerInfo.PlayerName;
            }

            GameDataUtils.gameDataMap[e.Game.Code].eventsLog.addVote(player, DateTime.Now, voted, e.VoteType);
        }

        [EventListener]
        public void onPlayerMurder(IPlayerMurderEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;

            var playerKilled = e.Victim;
            var currentGame = e.Game.Code;
            var killedClient = e.Game.Players.FirstOrDefault(p => p.Character == playerKilled);
            var killer = e.ClientPlayer;
            DateTime dateTime = DateTime.Now;

            if (killedClient != null && GameDataUtils.gameDataMap.ContainsKey(currentGame))
            {
                GameDataUtils.gameDataMap[currentGame].eventsLog.onDeathEvent(killedClient, dateTime, killer);
            }
        }




        [EventListener]
        public void onTaskCompletion(IPlayerCompletedTaskEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;

            if (e.ClientPlayer.Character == null) return;
            var player = e.ClientPlayer;
            var task = e.Task;
            var timeOfcompletion = DateTime.UtcNow;

            if (task.Complete)
            {
                GameDataUtils.gameDataMap[player.Game.Code].eventsLog.addTaskCompletion(player, timeOfcompletion, task.Task.Category, task.Task.Type);
            }
        }


        [EventListener]
        public void onDisconnection(IGamePlayerLeftEvent e)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(e.Game.Code)) return;
            var data = GameDataUtils.gameDataMap[e.Game.Code];
            if (data.Impostors.Contains(e.Player) && data.Impostors.Count == 2)
            {
                if (data.Impostors[0].Equals(e.Player))
                {
                    data.Impostors[0] = data.Impostors[1];
                    data.Impostors[1] = e.Player;
                }
            }
            DateTime dateTime = DateTime.Now;
            GameDataUtils.gameDataMap[e.Game.Code].eventsLog.disconnectedPlayer(dateTime, e.Player);
        }

        /* Moved to FileUtils
        private int RetrieveMatchIDFromFile(string path)
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
        */
        private void WriteToCsv(GameCode gameCode, string path, string matchFileName)
        {
            if (!GameDataUtils.gameDataMap.ContainsKey(gameCode)) return;

            string csvFilePath = Path.Combine(path, "matches.csv");
            var game = GameDataUtils.gameDataMap[gameCode];
            if (File.Exists(csvFilePath))
            {
                var file = File.Open(csvFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter writer = new StreamWriter(file);
                CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

                Season season = new Season();
                season.Id = game.matchId;
                season.Match = matchFileName;

                csvWriter.WriteRecord(season);
                csvWriter.NextRecord();
                csvWriter.Flush();
                writer.Flush();
                writer.Close();
                file.Close();
            }

        }

    }
}
