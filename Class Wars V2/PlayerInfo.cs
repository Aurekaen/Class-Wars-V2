using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Class_Wars_V2
{
    internal class PlayerInfo
    {
        private List<string> Players, ClassEdit, Arenas, Playing;
        private List<bool> Preview;
        private List<PlayerData> Backups;
        internal PlayerInfo()
        {
            Players = new List<string>();
            ClassEdit = new List<string>();
            Arenas = new List<string>();
            Playing = new List<string>();
            Preview = new List<bool>();
            Backups = new List<PlayerData>();
        }

        internal bool IsPreview(string player)
        {
            return Preview[GetPlayerIndex(player)];
        }

        internal string GetClassEdit(string player)
        {
            return ClassEdit[GetPlayerIndex(player)];
        }

        internal string GetArena(string player)
        {
            return Arenas[GetPlayerIndex(player)];
        }

        internal string GetClassPlaying(string player)
        {
            return Playing[GetPlayerIndex(player)];
        }

        internal int GetPlayerIndex(string player)
        {
            if (!Players.Contains(player))
            {
                Players.Add(player);
                ClassEdit.Add("none");
                Arenas.Add("none");
                Playing.Add("none");
                Preview.Add(false);
                Backups.Add(null);
            }
            return Players.FindIndex(p => p == player);
        }

        internal PlayerData GetBackup(string player)
        {
            return Backups[GetPlayerIndex(player)];
        }

        internal void SelectClass(string player, string c)
        {
            ClassEdit[GetPlayerIndex(player)] = c;
        }

        internal void SelectArena(string player, string a)
        {
            Arenas[GetPlayerIndex(player)] = a;
        }

        internal void SetPlaying(string player, string c)
        {
            Playing[GetPlayerIndex(player)] = c;
        }

        internal void SetPreviewing(string player, bool isPreview)
        {
            Preview[GetPlayerIndex(player)] = isPreview;
        }

        internal void SetBackup(TSPlayer player)
        {
            int index = GetPlayerIndex(player.Name);

            Backups[index] = new PlayerData(player); //I have no idea if both of these lines are necessary.
            Backups[index].CopyCharacter(player);
        }

        internal bool RestoreInv(TSPlayer player)
        {
            PlayerData b = GetBackup(player.Name);
            if (b == null)
                return false;
            b.RestoreCharacter(player);
            return true;
        }

        internal void GoodTeleport(TSPlayer player, TileLocation loc)
        {
            player.Teleport(loc.X * 16, loc.Y * 16);
        }
    }
}
