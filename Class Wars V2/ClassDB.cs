using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace Class_Wars_V2
{
    internal class ClassDB
    {
        private IDbConnection _db;

        internal ClassDB(IDbConnection db)
        {
            _db = db;
            var sqlCreator = new SqlTableCreator(_db,
                    _db.GetSqlType() == SqlType.Sqlite
                        ? (IQueryBuilder)new SqliteQueryCreator()
                        : new MysqlQueryCreator());
            var table = new SqlTable("Classes",
                new SqlColumn("Name", MySqlDbType.String) { Primary = true },
                new SqlColumn("Category", MySqlDbType.String),
                new SqlColumn("Description", MySqlDbType.String),
                new SqlColumn("Buffs", MySqlDbType.String),
                new SqlColumn("Itemgen", MySqlDbType.String),
                new SqlColumn("Auras", MySqlDbType.String),
                new SqlColumn("Inventory", MySqlDbType.String),
                new SqlColumn("MaxHealth", MySqlDbType.Int32),
                new SqlColumn("MaxMana", MySqlDbType.Int32),
                new SqlColumn("ExtraSlot", MySqlDbType.Int32),
                new SqlColumn("Locations", MySqlDbType.String),
                new SqlColumn("Statblob", MySqlDbType.String)
                );
            sqlCreator.EnsureTableStructure(table);
        }

        internal static ClassDB InitDb(string name)
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
            var tempDB = new ClassDB(db);
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

        internal void DeleteClass(string name)
        {
            Query("DELETE FROM Classes WHERE Name = @0", name);
        }

        internal void AddClass(CWClass x)
        {
            Query("INSERT INTO Classes (Name, Category, Description, Buffs, Itemgen, Auras, Inventory, MaxHealth, MaxMana, ExtraSlot, Locations, Statblob) " +
                "VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)", 
                x.name, x.category, Blob.Description(x.description), Blob.Buff(x.buffs), Blob.ItemRestock(x.items), Blob.Aura(x.auras)/*@5*/,
                Blob.Inventory(x.inventory), x.maxHealth, x.maxMana, x.extraSlot, Blob.LocationLinks(x.locations), Blob.Stat(x.stats));
        }

        internal void UpdateClass(CWClass x)
        {
            Query("UPDATE Classes SET Category = @0, Description = @1, Buffs = @2, Itemgen = @3, Auras = @4, Inventory = @5, MaxHealth = @6, MaxMana = @7, ExtraSlot = @8, Locations = @9, Statblob = @10 WHERE Name = @11",
                x.category, Blob.Description(x.description), Blob.Buff(x.buffs), Blob.ItemRestock(x.items), Blob.Aura(x.auras)/*@4*/,
                Blob.Inventory(x.inventory), x.maxHealth, x.maxMana, x.extraSlot, Blob.LocationLinks(x.locations), Blob.Stat(x.stats), x.name);
        }

        internal void LoadClasses(ref List<CWClass> list)
        {

            using (var reader = QueryReader("SELECT * FROM Classes"))
            {
                while(reader.Read())
                {
                    var name = reader.Get<string>("Name");
                    var category = reader.Get<string>("Category");
                    var description = reader.Get<string>("Description");
                    var buffs = reader.Get<string>("Buffs");
                    var itemgen = reader.Get<string>("Itemgen");
                    var auras = reader.Get<string>("Auras");
                    var inventory = reader.Get<string>("Inventory");
                    var maxHealth = reader.Get<int>("MaxHealth");
                    var maxMana = reader.Get<int>("MaxMana");
                    var extraSlot = reader.Get<int>("ExtraSlot");
                    var locations = reader.Get<string>("Locations");
                    var statblob = reader.Get<string>("Statblob");
                    list.Add(new CWClass(name, category, description, buffs, itemgen, auras, inventory, maxHealth, maxMana, extraSlot, locations, statblob));
                }
            }
        }
    }
}
