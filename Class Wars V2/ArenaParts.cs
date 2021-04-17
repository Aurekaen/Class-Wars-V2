using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_Wars_V2
{
    internal class TileLocation
    {
        internal int X, Y;
        internal TileLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    internal class BunkerTile
    {
        internal int ID, Wall, Paint;
        internal BunkerTile(int id, int wall, int paint)
        {
            ID = id;
            Wall = wall;
            Paint = paint;
        }
    }

    internal class ArenaStat
    {
        internal int BlueWins, RedWins, GameLength, MapKills, TotalDeaths;

        internal ArenaStat(int blueWins, int redWins, int gameLength, int mapKills, int totalDeaths)
        {
            BlueWins = blueWins;
            RedWins = redWins;
            GameLength = gameLength;
            MapKills = mapKills;
            TotalDeaths = totalDeaths;
        }

        internal ArenaStat()
        {
            BlueWins = 0;
            RedWins = 0;
            GameLength = 0;
            MapKills = 0;
        }
        internal int totalGames()
        {
            return BlueWins + RedWins;
        }

        internal string Wins()
        {
            string results = "Red Team: " + RedWins + ", or " + ((RedWins/totalGames())*100) +"% | Blue Team: " + BlueWins + ", or " + ((RedWins / totalGames()) * 100) + "%";
            return results;
        }

        internal static ArenaStat DeBlob(string blob)
        {
            ArenaStat results = new ArenaStat();
            string[] b = blob.Split('◙');
            try
            {
                 results = new ArenaStat(int.Parse(b[0]), int.Parse(b[1]), int.Parse(b[2]), int.Parse(b[3]), int.Parse(b[4]));
            }
            catch
            {
                Console.WriteLine("Warning: Error loading ArenaStat with BlueWins " + b[0] + " RedWins " + b[1] + " GameLength " + b[2] + " mapKills " + b[3] + " totalDeaths " + b[3]);
                Console.WriteLine("Skipping invalid ArenaStat and continuing");
            }
            return results;
        }

        internal static string Blob(ArenaStat preblob)
        {
            string blob = (preblob.BlueWins + "◙" + preblob.RedWins + "◙" + preblob.GameLength + "◙" + preblob.MapKills + "◙" + preblob.TotalDeaths);
            return blob;
        }
    }

    internal class Script
    {
        string Trigger, Flags, Command;
        internal Script(string trigger, string flags, string command)
        {
            Trigger = trigger;
            Flags = flags;
            Command = command;
        }

        internal static List<Script> DeBlob(string blob)
        {
            List<Script> results = new List<Script>();
            string[] a = blob.Split('☺');
            string[] b;

            foreach (string s in a)
            {
                b = s.Split('◙');
                try
                {
                    results.Add(new Script(b[0], b[1], b[2]));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Error loading Script with trigger " + b[0] + " flags " + b[1] + " command " + b[2]);
                    Console.WriteLine("Skipping invalid Script and continuing");
                }
            }
            return results;
        }

        internal static string Blob(List<Script> preblob)
        {
            List<string> blobules = new List<string>();
            foreach (Script s in preblob)
            {
                blobules.Add(s.Trigger + "◙" + s.Flags + "◙" + s.Command);
            }
            string blob = string.Join("☺", blobules);
            return blob;
        }
    }
}
