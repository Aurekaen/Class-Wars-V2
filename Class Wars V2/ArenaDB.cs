using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;
using Microsoft.Xna.Framework;

namespace Class_Wars_V2
{
    internal class ArenaDB
    {
        private IDbConnection _db;

        internal ArenaDB(IDbConnection db)
        {
            _db = db;
            var sqlCreator = new SqlTableCreator(_db,
                    _db.GetSqlType() == SqlType.Sqlite
                        ? (IQueryBuilder)new SqliteQueryCreator()
                        : new MysqlQueryCreator());
            var table = new SqlTable("CWArenas",
                new SqlColumn("Name", MySqlDbType.String) { Primary = true },
                new SqlColumn("HomeX", MySqlDbType.Int32),
                new SqlColumn("HomeY", MySqlDbType.Int32),
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32),
                new SqlColumn("ArenaTopLeftX", MySqlDbType.Int32),
                new SqlColumn("ArenaTopLeftY", MySqlDbType.Int32),
                new SqlColumn("ArenaBottomRightX", MySqlDbType.Int32),
                new SqlColumn("ArenaBottomRightY", MySqlDbType.Int32),
                new SqlColumn("RedBunkerTileID", MySqlDbType.Int32),
                new SqlColumn("BlueBunkerTileID", MySqlDbType.Int32),
                new SqlColumn("RedBunkerWallID", MySqlDbType.Int32),
                new SqlColumn("BlueBunkerWallID", MySqlDbType.Int32),
                new SqlColumn("RedBunkerPaintID", MySqlDbType.Int32),
                new SqlColumn("BlueBunkerPaintID", MySqlDbType.Int32),
                new SqlColumn("Stats", MySqlDbType.String),
                new SqlColumn("Scripts", MySqlDbType.String));
            sqlCreator.EnsureTableStructure(table);
        }

        internal static ArenaDB InitDb(string name)
        {
            IDbConnection db;
            if (TShock.Config.StorageType.ToLower() == "sqlite")
            {
                db =
                       new SqliteConnection(string.Format("uri=file://{0},Version=3",
                           Path.Combine(TShock.SavePath, name + ".sqlite")));
            }
            else if (TShock.Config.StorageType.ToLower() == "mysql")
            {
                try
                {
                    var host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword
                            )
                    };
                }
                catch (MySqlException x)
                {
                    TShock.Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
            }
            else
                throw new Exception("Invalid storage type.");
            var tempDB = new ArenaDB(db);
            return tempDB;
        }

        internal QueryResult QueryReader(string query, params object[] args)
        {
            return _db.QueryReader(query, args);
        }

        internal int Query(string query, params object[] args)
        {
            return _db.Query(query, args);
        }

        internal void DeleteArena(string name)
        {
            Query("DELETE From CWArenas WHERE Name = @0", name);
        }

        internal void UpdateArena(Arena a)
        {
            Query("UPDATE CWArenas SET HomeX = @0, HomeY = @1, RedSpawnX = @2, RedSpawnY = @3, BlueSpawnX = @4, BlueSpawnY = @5, ArenaTopLeftX = @6, " +
                "ArenaTopLeftY = @7, ArenaBottomRightX = @8, ArenaBottomRightY = @9, RedBunkerTileID = @10, BlueBunkerTileID = @11, RedBunkerWallID = @12, " +
                "BlueBunkerWallID = @13, RedBunkerPaintID = @14, BlueBunkerPaintID = @15, Stats = @16, Scripts = @17 WHERE Name = @18"
                ,a.Home.X, a.Home.Y, a.RedSpawn.X, a.RedSpawn.Y, a.BlueSpawn.X, a.BlueSpawn.Y, a.TopLeft.X, a.TopLeft.Y, a.BottomRight.X, a.BottomRight.Y, a.Red.ID,
                a.Blue.ID, a.Red.Wall, a.Blue.Wall, a.Red.Paint, a.Blue.Paint, ArenaStat.Blob(a.Stats), Script.Blob(a.Scripts), a.Name);
        }

        internal void AddArena(Arena a)
        {
            Query("INSERT INTO CWArenas (Name, HomeX, HomeY, RedSpawnX, RedSpawnY, BlueSpawnX, BlueSpawnY, ArenaTopLeftX, ArenaTopLeftY, ArenaBottomRightX, ArenaBottomRightY, " +
                "RedBunkerTileID, BlueBunkerTileID, RedBunkerWallID, BlueBunkerWallID, RedBunkerPaintID, BlueBunkerPaintID, Stats, Scripts) " +
                "VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18)",
                a.Name, a.Home.X, a.Home.Y, a.RedSpawn.X, a.RedSpawn.Y, a.BlueSpawn.X, a.BlueSpawn.Y, a.TopLeft.X, a.TopLeft.Y, a.BottomRight.X, a.BottomRight.Y, a.Red.ID,
                a.Blue.ID, a.Red.Wall, a.Blue.Wall, a.Red.Paint, a.Blue.Paint, ArenaStat.Blob(a.Stats), Script.Blob(a.Scripts));
        }

        internal void LoadArenas(ref List<Arena> list)
        {
            using (var reader = QueryReader("SELECT * FROM CWArenas"))
            {
                while(reader.Read())
                {
                    var name = reader.Get<string>("Name");
                    var homeX = reader.Get<int>("HomeX");
                    var homeY = reader.Get<int>("HomeY");
                    var redSpawnX = reader.Get<int>("RedSpawnX");
                    var redSpawnY = reader.Get<int>("RedSpawnY");
                    var blueSpawnX = reader.Get<int>("BlueSpawnX");
                    var blueSpawnY = reader.Get<int>("BlueSpawnY");
                    var arenaTopLeftX = reader.Get<int>("ArenaTopLeftX");
                    var arenaTopLeftY = reader.Get<int>("ArenaTopLeftY");
                    var arenaBottomRightX = reader.Get<int>("ArenaBottomRightX");
                    var arenaBottomRightY = reader.Get<int>("ArenaBototmRightY");
                    var redBunkerTileID = reader.Get<int>("RedBunkerTileID");
                    var blueBunkerTileID = reader.Get<int>("BlueBunkerTileID");
                    var redBunkerWallID = reader.Get<int>("RedBunkerWallID");
                    var blueBunkerWallID = reader.Get<int>("BlueBunkerWallID");
                    var redBunkerPaintID = reader.Get<int>("RedBunkerPaintID");
                    var blueBunkerPaintID = reader.Get<int>("BlueBunkerPaintID");
                    var stats = reader.Get<string>("Stats");
                    var scripts = reader.Get<string>("Scripts");

                    list.Add(new Arena(name, homeX, homeY, redSpawnX, redSpawnY, blueSpawnX, blueSpawnY, arenaTopLeftX, arenaTopLeftY, 
                        arenaBottomRightX, arenaBottomRightY, redBunkerTileID, redBunkerWallID, redBunkerPaintID, blueBunkerTileID, 
                        blueBunkerWallID, blueBunkerPaintID, stats, scripts));
                }
            }
        }


    }
}
