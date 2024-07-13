using Impostor.Api.Innersloth.GameOptions;
using Impostor.Api.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AURankedPlugin.Models
{
    public class GameData
    {


        //private string gameCode { get; set; }
        public List<IClientPlayer> Players { get; set; }
        public List<IClientPlayer> Crewmates { get; set; }
        public List<IClientPlayer> Impostors { get; set; }
        public List<IClientPlayer> DeadPlayers { get; set; }
        private ILogger _logger { get; set; }
        public EventsLog eventsLog { get; set; }
        private string gameCode { get; set; }
        public int countdown { get; set; }
        public bool canceled { get; set; }
        public bool inMeeting = false;
        public int matchId { get; set; }
        public NormalGameOptions gameOptions { get; set; }


        public GameData(ILogger logger, string code, int id) 
        {
            Players = new List<IClientPlayer>();
            Crewmates = new List<IClientPlayer>();
            Impostors = new List<IClientPlayer>();
            DeadPlayers = new List<IClientPlayer>();
            gameCode = code;
            eventsLog = new EventsLog(code);
            _logger = logger;
            matchId = id;
            gameOptions = new NormalGameOptions();

        }

        public GameData(NormalGameOptions options, string code) 
        {
            Players = new List<IClientPlayer>();
            Crewmates = new List<IClientPlayer>();
            Impostors = new List<IClientPlayer>();
            DeadPlayers = new List<IClientPlayer>();
            gameCode = code;
            eventsLog = new EventsLog(code);
            _logger = null;
            matchId = -1;
            gameOptions = new NormalGameOptions();
        }

        public void setOptions(NormalGameOptions options) 
        {
            gameOptions = options;
        }

        public void AddPlayer(IClientPlayer player) 
        {
            Players.Add(player);

           
            var playerInfo = player.Character?.PlayerInfo;
            if (playerInfo != null) 
            {
                if(playerInfo.IsImpostor && !playerInfo.IsDead) 
                {
                    Impostors.Add(player);
                } else 
                {
                    Crewmates.Add(player);
                }
            } else 
            {
                //Shouldn't happen
                _logger.LogError($"PlayerInfo is null on obj {player.Character}");

            }
        }

        public void AddPlayers(List<IClientPlayer> players) 
        {
            foreach(var player in players)
            {
                Players.Add(player);
                var playerInfo = player.Character?.PlayerInfo;

                if (playerInfo != null)
                {
                    if (playerInfo.IsImpostor && !playerInfo.IsDead)
                    {
                        Impostors.Add(player);
                    }
                    else
                    {
                        Crewmates.Add(player);
                    }
                }
                else
                {
                    //Shouldn't happen
                    _logger.LogError($"PlayerInfo is null on obj {player.Character}");

                }
            }
        }

        public void RegisterDeadPlayer(IClientPlayer player) 
        {
            var playerInfo = player.Character?.PlayerInfo;
            if (playerInfo != null) 
            {
                Players.Remove(player);
                if(playerInfo.IsImpostor) 
                {
                    Impostors.Remove(player);
                } else 
                {
                    Crewmates.Remove(player);
                }
                DeadPlayers.Add(player);
            }
        }

        public List<string> stringifyData()
        {
            string playerNames = string.Join(",", Players.Select(p =>
            {
                string playerName = p.Character.PlayerInfo.PlayerName;
                if (playerName == "\u003C#FD0\u003EVex" || playerName == "<#FD0>Vex")
                {
                    return "Vex";
                }
                return playerName;
            }));

            //string impostor = Impostor.Character.PlayerInfo.PlayerName;
            string impostors = string.Join(", ", Impostors.Select(p =>
            {
                string impostor = p.Character.PlayerInfo.PlayerName;
                if (impostor == "\u003C#FD0\u003EVex" || impostor == "<#FD0>Vex")
                {
                    return "Vex";
                }
                return impostor;
            }));

            return new List<string> { eventsLog.gameStartedUTC.ToString(), playerNames, impostors };
        }

        public string jsonifyRawData()
        {
            // Assume this is your list of strings
            // Create a list to hold the parsed data
            List<Dictionary<string, object>> data = new();

            // Parse each string and add the data to the list
            foreach (string str in eventsLog.getEventsLog())
            {
                var parts = str.Split(" | ");
                /*var item = new Dictionary<string, object>
                {
                    { "time", parts[0].Trim() },
                    { "name", parts[1].Trim() },
                    { "currentCount", int.Parse(parts[2].Trim()) }
                };*/
                var item = new Dictionary<string, object>();
                switch (parts[0].Trim())
                {
                    case "0":
                        item.Add("Event", "StartGame");
                        item.Add("Time", parts[1].Trim());
                        item.Add("GameCode", parts[2].Trim());
                        break;
                    case "1":
                        item.Add("Event", "Task");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Name", parts[2].Trim());
                        item.Add("TaskType", parts[3].Trim());
                        item.Add("TaskName", parts[4].Trim());
                        break;
                    case "2":
                        item.Add("Event", "Death");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Name", parts[2].Trim());
                        item.Add("Killer", parts[3].Trim());
                        break;
                    case "3":
                        item.Add("Event", "FinalHide");
                        item.Add("Time", parts[1].Trim());
                        break;
                    case "4":
                        item.Add("Event", "EndGame");
                        item.Add("Time", parts[1].Trim());
                        item.Add("WinReason", parts[2].Trim());
                        item.Add("GameCode", parts[3].Trim());
                        break;
                    case "5":
                        item.Add("Event", "MeetingStart");
                        item.Add("Time", parts[1].Trim());
                        item.Add("GameCode", parts[2].Trim());
                        item.Add("Player", parts[3].Trim());
                        break;
                    case "6":
                        item.Add("Event", "MeetingEnd");
                        item.Add("Time", parts[1].Trim());
                        item.Add("GameCode", parts[2].Trim());
                        item.Add("Result", parts[3].Trim());
                        break;
                    case "7":
                        item.Add("Event", "PlayerVote");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Player", parts[2].Trim());
                        item.Add("Target", parts[3].Trim());
                        item.Add("Type", parts[4].Trim());
                        break;
                    case "8":
                        item.Add("Event", "GameCancel");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Player", parts[2].Trim());
                        break;
                    case "9":
                        item.Add("Event", "BodyReport");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Player", parts[2].Trim());
                        item.Add("DeadPlayer", parts[3].Trim());
                        break;
                    case "10":
                        item.Add("Event", "ManualGameEnd");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Player", parts[2].Trim());
                        break;
                    case "11":
                        item.Add("Event", "Exiled");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Player", parts[2].Trim());
                        break;
                    case "99":
                        item.Add("Event", "Disconnect");
                        item.Add("Time", parts[1].Trim());
                        item.Add("Name", parts[2].Trim());
                        break;
                    default:
                        item.Add("Event", "ERROR");
                        break;
                }

                data.Add(item);
            }

            // Serialize the data to JSON
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            return json;
        }

        
        public void DecreaseCountdown(int delta)
        {
            countdown -= delta;
        }

        public void DecreaseCountdownByOne()
        {
            countdown -= 1;
        }
        


        public void Reset() 
        {
            Players = new List<IClientPlayer>();
            Crewmates = new List<IClientPlayer>();
            Impostors = new List<IClientPlayer>();
            DeadPlayers = new List<IClientPlayer>();
            eventsLog = new EventsLog(gameCode);
        }

    }
}
