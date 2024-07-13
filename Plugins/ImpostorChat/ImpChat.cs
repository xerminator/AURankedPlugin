using AURankedPlugin.Models;
using Impostor.Api.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AURankedPlugin.Plugins.ImpostorChat
{
    internal class ImpChat
    {

        public Dictionary<IClientPlayer, bool> active { get; set; }
        private GameData game { get; set; }

        public ImpChat(GameData gameData) 
        {
            active = new Dictionary<IClientPlayer, bool>();
            game = gameData;
            AddPlayers(game.Impostors);
            
        }

        public void AddPlayers(List<IClientPlayer> players)
        {
            
            foreach(var player in players)
            {
                if (player.Character == null) return;
                if (player.Character.PlayerInfo.IsImpostor)
                {
                    active.Add(player, false);
                }
            }

        }

        public void Activate(IClientPlayer player)
        {
            active[player] = true;
            player.Character?.SendChatToPlayerAsync($"<#24b95d>You are now in Impostor Chat mode.");
        }

        public void Deactivate(IClientPlayer player) 
        {
            active[player] = false;
            player.Character?.SendChatToPlayerAsync($"<#24b95d>You are no longer in Impostor Chat mode.");
        }

        public void Reset()
        {
            active = new Dictionary<IClientPlayer, bool>();
        }


    }
}
