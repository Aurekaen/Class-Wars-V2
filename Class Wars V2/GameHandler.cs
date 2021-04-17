using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Class_Wars_V2
{
    internal class GameHandler
    {
        List<TSPlayer> RedTeam, BlueTeam;
        List<int> allPlayers;
        Arena SelectedArena;
        List<KeyValuePair<int, bool>> Readied;
        bool VoteStarted;
        bool GameInProgress;
        PlayerInfo Players = new PlayerInfo();
        ClassUtils Classes = new ClassUtils();

        internal GameHandler(ref PlayerInfo p, ref ClassUtils c)
        {
            Players = p;
            Classes = c;
            RedTeam = new List<TSPlayer>();
            BlueTeam = new List<TSPlayer>();
            allPlayers = new List<int>();
        }

        internal void JoinBlue(TSPlayer player)
        {
            if (!allPlayers.Contains(player.Index))
                allPlayers.Add(player.Index);
            RedTeam.RemoveAll(p => p.Index == player.Index);
            BlueTeam.RemoveAll(p => p.Index == player.Index);
            BlueTeam.Add(player);
            Main.player[player.Index].team = 3;
            player.SendInfoMessage("You have joined Blue team for Class Wars.");
        }

        internal void JoinRed(TSPlayer player)
        {
            if (!allPlayers.Contains(player.Index))
                allPlayers.Add(player.Index);
            RedTeam.RemoveAll(p => p.Index == player.Index);
            BlueTeam.RemoveAll(p => p.Index == player.Index);
            RedTeam.Add(player);
            Main.player[player.Index].team = 1;
            player.SendInfoMessage("You have joined Red team for Class Wars.");
        }

        protected TSPlayer GetTSPlayer(int index)
        {
            return TShock.Players.First(p => p.Index == index);
        }

        internal void StartVoting(string name, Arena selected)
        {
            SelectedArena = selected;
            TShock.Utils.Broadcast(name + " has started a game of Class Wars. Type /cw join <team> to join!", Color.Green);
            VoteStarted = true;
        }

        internal void StartGame()
        {

        }

        internal bool TeamsExist()
        {
            return (BlueTeam.Count > 0 && RedTeam.Count > 0);
        }

        internal string TeamsBalanced()
        {
            if (BlueTeam.Count == RedTeam.Count)
            {
                return "Teams are unbalanced, " + BlueTeam.Count + " players on blue team, " + RedTeam.Count + " players on red team.";
            }
            switch(HasMiner(BlueTeam))
            {
                case 0:
                    return "Blue team does not have a miner.";
                case 1:
                    break;
                default:
                    return "Blue team has multiple miners.";
            }
            switch (HasMiner(RedTeam))
            {
                case 0:
                    return "Red team does not have a miner.";
                case 1:
                    break;
                default:
                    return "Red team has multiple miners.";
            }
            return "good";
        }

        internal int HasMiner(List<TSPlayer> team)
        {
            int MinerCount = 0;
            foreach(TSPlayer p in team)
            {
                if (Classes.GetClass(Players.GetClassPlaying(p.Name)).category == "Miner")
                    MinerCount++;
            }
            return MinerCount;
        }

        internal void GTFO(TSPlayer player)
        {
            allPlayers.RemoveAll(p => p == player.Index);
            RedTeam.RemoveAll(p => p.Index == player.Index);
            BlueTeam.RemoveAll(p => p.Index == player.Index);
        }

        internal void OnUpdate(EventArgs args)
        {
            if (GameInProgress)
            {
                foreach (TSPlayer plr in RedTeam)
                {
                    //Forces PVP for both teams for the duration of the game.
                    plr.TPlayer.hostile = true;
                    NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, plr.Index, 0f, 0f, 0f);
                }
                foreach (TSPlayer plr in BlueTeam)
                {
                    plr.TPlayer.hostile = true;
                    NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, plr.Index, 0f, 0f, 0f);
                }
            }
        }
    }
}
