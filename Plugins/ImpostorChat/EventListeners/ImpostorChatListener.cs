using AURankedPlugin.Models;
using AURankedPlugin.Utils;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AURankedPlugin.Plugins.ImpostorChat.EventListeners
{
    public class ImpostorChatListener : IEventListener
    {
        private readonly ILogger<ImpostorChatPlugin> _logger;
        private ImpChat impChat { get; set; }
        //private Dictionary<GameCode, GameData> gameDataMap = new Dictionary<GameCode, GameData>();


        public ImpostorChatListener(ILogger<ImpostorChatPlugin> logger)
        {
            _logger = logger;
            impChat = null;
        }

   
        /*
        [EventListener]
        public void onGameStarted(IGameStartedEvent e)
        {
            //string path = Path.Combine(Environment.CurrentDirectory, "plugins", "MatchLogs", _config.seasonName);
            //int Id = FileUtils.RetrieveMatchIDFromFile(path) + 1;
            //var gameData = new GameData(_logger, e.Game.Code, -1);
            //foreach (var player in e.Game.Players)
            //{
            //    gameData.AddPlayer(player, player.Character.PlayerInfo.IsImpostor);
            //}
            //gameDataMap.Add(e.Game.Code, gameData);
            
        }
        */

        
        [EventListener]
        public void onGameEnd(IGameEndedEvent e)
        {
            GameDataUtils.gameDataMap[e.Game.Code].Reset();
            GameDataUtils.gameDataMap.Remove(e.Game.Code);
            impChat.Reset();
        }
        

        [EventListener]
        public async void onPlayerChat(IPlayerChatEvent e)
        {
            if (impChat == null)
            {
                impChat = new ImpChat(GameDataUtils.gameDataMap[e.Game.Code]);
            }
            if (impChat.active.Count == 0) 
            {
                impChat.AddPlayers(e.Game.Players.ToList());
            }

            bool startsWithPrefix = e.Message.StartsWith("/") || e.Message.StartsWith("?");
            if (!startsWithPrefix) return;
            if (e.Game.GameState == GameStates.NotStarted) return;
            if (e.ClientPlayer == null || e.ClientPlayer.Character == null) return;
            string command = e.Message.Split(" ")[0].Trim().ToLower();
            string doublePrefix = "";
            bool isCommand = true;
            if (e.Message.Length < 2) return;
            List<string> allowed = new List<string> { "//", "??", "/w", "?w", "/say", "?say" };
            
            if (!allowed.Contains(command))
            {
                doublePrefix = e.Message.Substring(0, 2).ToLower();
                isCommand = false;
            }
            if (allowed.Contains(doublePrefix))
            {
                isCommand = true;
                command = doublePrefix;
            }
            if (!isCommand)
            {
                return;
            }
            e.IsCancelled = true;
            
            if (impChat.active[e.ClientPlayer])
            {
                impChat.Activate(e.ClientPlayer);
            } else 
            {
                impChat.Deactivate(e.ClientPlayer);
            }

            var impostors = GameDataUtils.gameDataMap[e.Game.Code].Impostors;

            

            if (!impostors.Contains(e.ClientPlayer))
            {
                return;
            }
            var message = "";
            switch (command)
            {
                case "//":
                    message = e.Message.Remove(0, 2);
                    break;
                case "??":
                    message = e.Message.Remove(0, 2);
                    break;
                case "/w":
                    message = e.Message.Remove(0, 3);
                    break;
                case "?w":
                    message = e.Message.Remove(0, 3);
                    break;
                default:
                    message = e.Message.Remove(0, 5);
                    break;
            }


            foreach (var player in impostors)
            {
                if (player == null) return;
                if (player != e.ClientPlayer)
                {
                    //string reciever_msg = "From " + e.ClientPlayer.Character.PlayerInfo.PlayerName + ":\n" + "<#c51111>" + message;
                    string reciever_msg = "<#c51111>" + message;
                    if (e.ClientPlayer.Character.PlayerInfo.IsDead && player.Character != null)
                    {
                        reciever_msg = "From " + e.ClientPlayer.Character.PlayerInfo.PlayerName + ":\n" + "<#c51111>" + message;
                        await player.Character.SendChatToPlayerAsync(reciever_msg, player.Character);
                    }
                    else
                    {
                        await e.ClientPlayer.Character.SendChatToPlayerAsync(reciever_msg, player.Character);
                    }
                }
            }


        }
    }
}
