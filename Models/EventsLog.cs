using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AURankedPlugin.Models
{
    public class EventsLog
    {

        private List<string> eventsLog { get; set; }
        private string gameCode { get; set; }
        public DateTime gameStartedUTC { get; set; }
        public string gameStarted { get; set; }

        public EventsLog(string code) 
        {
            eventsLog = new List<string>();
            gameCode = code;
            
        }

        public List<string> getEventsLog() 
        {
            return eventsLog;
        }

        public void gameStartTimeSet()
        {
            var timeStart = DateTime.Now;
            gameStartedUTC = timeStart;
            gameStarted = $"0 | {timeStart} | {gameCode}";
            eventsLog.Add(gameStarted);

        }

        public void addTaskCompletion(IClientPlayer player, DateTime timeOfCompletion, TaskCategories taskType, TaskTypes taskName)
        {
            eventsLog.Add($"1 | {timeOfCompletion} | {player.Character?.PlayerInfo.PlayerName} | {taskType} | {taskName}");
        }

        public void onDeathEvent(IClientPlayer player, DateTime timeOfDeath, IClientPlayer killer)
        {
            eventsLog.Add($"2 | {timeOfDeath} | {player.Character?.PlayerInfo.PlayerName} | {killer.Character?.PlayerInfo.PlayerName}");
        }

/*        public void finalHideStarted(DateTime finalHideStart)
        {
            eventsLog.Add($"3 | {finalHideStart}");
        }*/

        public void gameEnd(DateTime endOfGame, string endReason)
        {
            eventsLog.Add($"4 | {endOfGame} | {endReason} | {gameCode}");
        }

        public void startMeeting(string playername, DateTime timeOfMeetingStart)
        {
            eventsLog.Add($"5 | {timeOfMeetingStart} | {gameCode} | {playername}");
        }

        public void endMeeting(DateTime timeOfMeetingEnd, string result)
        {
            eventsLog.Add($"6 | {timeOfMeetingEnd} | {gameCode} | {result}");
        }

        public void addVote(string playername, DateTime timeOfVote, string voted, VoteType voteType) {
            eventsLog.Add($"7 | {timeOfVote} | {playername} | {voted} | {voteType}");
        }

        public void onCancel(string playername, DateTime timeOfCancel)
        {
            eventsLog.Add($"8 | {timeOfCancel} | {playername}");
        }

        public void onReport(string playername, string dead_playername, DateTime timeOfBodyReport)
        {
            eventsLog.Add($"9 | {timeOfBodyReport} | {playername} | {dead_playername} |");
        }

        public void onEnd(IClientPlayer player, DateTime endTime)
        {
            eventsLog.Add($"10 | {endTime} | {player.Character?.PlayerInfo.PlayerName}");
        }

        public void onExile(string exiled, DateTime exileTime)
        {
            eventsLog.Add($"11 | {exileTime} | {exiled}");
        }

        public void disconnectedPlayer(DateTime dcTime, IClientPlayer player)
        {
            eventsLog.Add($"99 | {dcTime} | {player.Character?.PlayerInfo.PlayerName}");
        }



    }
}
