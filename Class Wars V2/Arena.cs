using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_Wars_V2
{
    internal class Arena
    {
        internal string Name;
        internal TileLocation Home, RedSpawn, BlueSpawn, TopLeft, BottomRight;
        internal BunkerTile Red, Blue;
        internal ArenaStat Stats;
        internal List<Script> Scripts;

        internal Arena(string name, int homeX, int homeY, int redSpawnX, int redSpawnY, int blueSpawnX, int blueSpawnY, int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, int redID, int redWall, int redPaint, int blueID, int blueWall, int bluePaint, string stats, string scriptBlob)
        {
            Name = name;
            Home = new TileLocation(homeX, homeY);
            RedSpawn = new TileLocation(redSpawnX, redSpawnY);
            BlueSpawn = new TileLocation(blueSpawnX, blueSpawnY);
            TopLeft = new TileLocation(topLeftX, topLeftY);
            BottomRight = new TileLocation(bottomRightX, bottomRightY);
            Red = new BunkerTile(redID, redWall, redPaint);
            Blue = new BunkerTile(blueID, blueWall, bluePaint);
            Stats = ArenaStat.DeBlob(stats);
            Scripts = Script.DeBlob(scriptBlob);
        }

        internal Arena(string name, TileLocation home, TileLocation redSpawn, TileLocation blueSpawn, TileLocation topLeft, TileLocation bottomRight, BunkerTile red, BunkerTile blue, ArenaStat stats = null, List<Script> scripts = null)
        {
            Name = name;
            Home = home;
            RedSpawn = redSpawn;
            BlueSpawn = blueSpawn;
            TopLeft = topLeft;
            BottomRight = bottomRight;
            Red = red;
            Blue = blue;
            if (stats == null)
                Stats = new ArenaStat();
            else
                Stats = stats;
            if (scripts == null)
                Scripts = new List<Script>();
            else
                Scripts = scripts;
        }

        internal Arena(string name)
        {
            Name = name;
        }
    }

    internal class ArenaUtils
    {
        internal List<Arena> arenas = new List<Arena>();
        internal ArenaDB arena_db;

        internal ArenaUtils()
        {
            arena_db = ArenaDB.InitDb("Classes");
            arena_db.LoadArenas(ref arenas);
        }

        internal Arena GetArena(string name)
        {
            return arenas[arenas.FindIndex(a => a.Name == name)];
        }

        internal string ArenaExists(string lookup, ref int resultType)
        {
            lookup = lookup.ToLower();
            string results = "none";
            int resultCount = 0;
            foreach (Arena a in arenas)
            {
                if (a.Name.ToLower() == lookup)
                {
                    resultType = 0;
                    return a.Name;
                }
                if (a.Name.ToLower().Contains(lookup))
                {
                    if (resultCount == 0)
                    {
                        results = a.Name;
                    }
                    else
                    {
                        results = string.Join(", ", a.Name);
                    }
                    resultCount++;
                }
            }
            switch (resultCount)
            {
                case 0:
                    resultType = -1;
                    results = "Arena " + lookup + " was not found.";
                    break;
                case 1:
                    resultType = 1;
                    break;
                default:
                    resultType = 2;
                    break;
            }
            return results;
        }

        internal void AddArena(string name)
        {
            Arena a = new Arena(name);
            arenas.Add(a);
            arena_db.AddArena(a);
        }

        internal void UpdateArena(Arena newArena)
        {
            arenas[arenas.FindIndex(a => a.Name == newArena.Name)] = newArena;
            arena_db.UpdateArena(newArena);
        }

        internal bool DelArena(string name)
        {
            foreach (Arena a in arenas)
            {
                if (a.Name == name)
                {
                    arena_db.DeleteArena(a.Name);
                    arenas.Remove(a);
                    return true;
                }
            }
            return false;
        }
    }
}
