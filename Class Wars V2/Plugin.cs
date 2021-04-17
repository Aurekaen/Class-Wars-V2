using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Globalization;

namespace Class_Wars_V2
{
    [ApiVersion(2, 1)]
    public class TestPlugin : TerrariaPlugin
    {
        #region plugin info
        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author => "Alec Suehrstedt";

        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description => "Automatic Class Wars hosting";

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "Class Wars v2";

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => new Version(1, 0, 0, 0);

        /// <summary>
        /// Initializes a new instance of the TestPlugin class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public TestPlugin(Main game) : base(game)
        {
            Order = 10;
        }
        #endregion

        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary>
        internal PlayerInfo PInfo;
        internal ClassUtils CUtil;
        internal ArenaUtils AUtil;
        internal GameHandler CWGame;
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string> { "cw.manage" }, Arena, "arena", "ar"));
            Commands.ChatCommands.Add(new Command(new List<string> { "cw.manage" }, ClassMod, "cwmod", "cwm"));
            Commands.ChatCommands.Add(new Command(new List<string> { "cw.manage" }, ClassWars, "cw", "classwars"));
            Commands.ChatCommands.Add(new Command(new List<string> { "cw.manage" }, ClassAccess, "class", "cs"));
            PInfo = new PlayerInfo();
            CUtil = new ClassUtils();
            AUtil = new ArenaUtils();
            CWGame = new GameHandler(ref PInfo, ref CUtil);
            ServerApi.Hooks.GameUpdate.Register(this, CWGame.OnUpdate);
        }

        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, CWGame.OnUpdate);
            }
            base.Dispose(disposing);
        }


        private void Arena(CommandArgs args)
        {
            TSPlayer player = args.Player;
            #region help
            if (args.Parameters.Count == 0 || args.Parameters[0] == "help")
            {
                player.SendErrorMessage("Aliases: /arena, /ar");
                player.SendErrorMessage("/arena select [arena name]");
                player.SendErrorMessage("/arena add [arena name]");
                player.SendErrorMessage("/arena del");
                player.SendErrorMessage("/arena goto <arena name>");
                player.SendErrorMessage("/arena set {home | redspawn| bluespawn}");
                player.SendErrorMessage("/arena bounds {1 | 2 | define}");
                player.SendErrorMessage("/arena stats <arena name>");
                player.SendErrorMessage("/arena script {add | del | clear} [script]");
                return;
            }
            #endregion

            #region add
            if (args.Parameters[0].ToLower() == "add")
            {
                if (args.Parameters.Count == 1)
                {
                    player.SendErrorMessage("Usage: /arena add [arena name]");
                    player.SendErrorMessage("This adds a new arena using the specified name.");
                    return;
                }
                args.Parameters.RemoveAt(0);
                int resultType = -100;
                string name = string.Join(" ", args.Parameters);
                string results = AUtil.ArenaExists(name, ref resultType);
                if (resultType == 0)
                {
                    player.SendErrorMessage("There is already an arena named " + name);
                    return;
                }
                AUtil.AddArena(name);
                player.SendSuccessMessage("Arena " + name + " successfully added.");
                PInfo.SelectArena(player.Name, name);
                player.SendSuccessMessage("Arena " + name + " has been automatically selected.");
                return;
            }
            #endregion

            #region select
            if (args.Parameters[0].ToLower() == "select")
            {
                if (args.Parameters.Count == 1)
                {
                    player.SendErrorMessage("Usage: /arena select [arena name]");
                    player.SendErrorMessage("This selects an arena for other arena modifying commands to reference");
                    return;
                }
                int resultType = 0;
                args.Parameters.RemoveAt(0);
                string search = string.Join(" ", args.Parameters);
                string results = AUtil.ArenaExists(search, ref resultType);
                switch (resultType)
                {
                    case -1:
                        player.SendErrorMessage(results);
                        break;
                    case 0:
                    case 1:
                        PInfo.SelectArena(player.Name, results);
                        player.SendSuccessMessage("Arena " + results + " selected.");
                        break;
                    case 2:
                        player.SendErrorMessage("Multiple arenas found: ");
                        player.SendErrorMessage(results);
                        break;
                }
                return;
            }
            #endregion

            #region checkSelected
            string selectedArena = PInfo.GetArena(player.Name);
            if (selectedArena == "none" && args.Parameters[0].ToLower() != "goto" && args.Parameters[0].ToLower() != "stats")
            {
                player.SendErrorMessage("Please select an arena using \"/arena select\"before using this command.");
                return;
            }
            Arena a = AUtil.GetArena(selectedArena);
            #endregion

            #region del
            if (args.Parameters[0].ToLower() == "del")
            {
                if (AUtil.DelArena(a.Name))
                {
                    player.SendSuccessMessage("Arena " + a.Name + " successfully deleted.");
                }
                else
                {
                    player.SendErrorMessage("Warning: Unknown arena selected: " + a.Name);
                    player.SendErrorMessage("Deselecting unknown arena.");
                }
                PInfo.SelectArena(player.Name, "none");
                return;
            }
            #endregion

            #region goto
            if (args.Parameters[0].ToLower() == "goto")
            {
                if (args.Parameters.Count == 1)
                {
                    if (selectedArena == "none")
                    {
                        player.SendErrorMessage("Please select an arena or specify one by using /arena goto [arena name]");
                        return;
                    }
                }
                else
                {
                    int resultType = -100;
                    args.Parameters.RemoveAt(0);
                    string search = string.Join(" ", args.Parameters);
                    string results = AUtil.ArenaExists(search, ref resultType);
                    switch (resultType)
                    {
                        case -1:
                            player.SendErrorMessage(results);
                            return;
                        case 0:
                        case 1:
                            a = AUtil.GetArena(results);
                            break;
                        case 2:
                            player.SendErrorMessage("Multiple arenas found: ");
                            player.SendErrorMessage(results);
                            return;
                    }

                }

                if (a.Home == null || (a.Home.X == 0 && a.Home.Y == 0))
                {
                    player.SendErrorMessage("Arena " + a.Name + " does not have a home defined yet. Please use \'/arena set home\' before attempting to teleport to it.");
                    return;
                }
                PInfo.GoodTeleport(player, a.Home);
                player.SendSuccessMessage("You have been teleported to " + a.Name);
                return;
            }
            #endregion

            #region set
            if (args.Parameters[0].ToLower() == "set")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[0].ToLower() != "home" && args.Parameters[0].ToLower() != "redspawn" && args.Parameters[0].ToLower() != "rs" && args.Parameters[0].ToLower() != "bluespawn" && args.Parameters[0].ToLower() != "bs"))
                {
                    player.SendErrorMessage("Usage: /arena set {home | redspawn | bluespawn}");
                    player.SendErrorMessage("This command sets the current arena's home/redspawn/bluespawn position to your current location.");
                    player.SendErrorMessage("redspawn and bluespawn can be shortened to rs and bs respecively.");
                    return;
                }
                switch (args.Parameters[1].ToLower())
                {
                    case "home":
                        a.Home = new TileLocation(player.TileX, player.TileY);
                        player.SendErrorMessage(a.Name + " has had its home set to your current location.");
                        AUtil.UpdateArena(a);
                        return;
                    case "rs":
                    case "redspawn":
                        a.RedSpawn = new TileLocation(player.TileX, player.TileY);
                        player.SendErrorMessage(a.Name + " has had its red spawn set to your current location.");
                        AUtil.UpdateArena(a);
                        return;
                    case "bs":
                    case "bluespawn":
                        a.BlueSpawn = new TileLocation(player.TileX, player.TileY);
                        player.SendErrorMessage(a.Name + " has had its blue spawn set to your current location.");
                        AUtil.UpdateArena(a);
                        return;
                }
                player.SendErrorMessage("Usage: /arena set {home | redspawn | bluespawn}");
                player.SendErrorMessage("This command sets the current arena's home/redspawn/bluespawn position to your current location.");
                player.SendErrorMessage("redspawn and bluespawn can be shortened to rs and bs respecively.");
                return;
            }
            #endregion

            #region bounds
            if (args.Parameters[0].ToLower() == "bounds")
            {
                if (args.Parameters.Count == 0 || (args.Parameters[1] != "define" && args.Parameters[1] != "1" && args.Parameters[1] != "2"))
                {
                    player.SendErrorMessage("Usage: /arena bounds {1 | 2 | define}");
                    player.SendErrorMessage("This command is used to set two opposing corners, defining a rectangle which the arena is contained within.");
                    return;
                }
                if (args.Parameters[1] == "define")
                {
                    TileLocation topLeft = new TileLocation((Math.Min(player.TempPoints[0].X, player.TempPoints[1].X)), (Math.Min(player.TempPoints[0].Y, player.TempPoints[1].Y)));
                    TileLocation bottomRight = new TileLocation((Math.Max(player.TempPoints[0].X, player.TempPoints[1].X)), (Math.Max(player.TempPoints[0].Y, player.TempPoints[1].Y)));
                    a.TopLeft = topLeft;
                    a.BottomRight = bottomRight;
                    AUtil.UpdateArena(a);
                    player.SendSuccessMessage("Boundaries defined for " + a.Name + ".");
                    return;
                }
                if (args.Parameters[1] == "1")
                {
                    player.SendInfoMessage("Select point 1");
                    player.AwaitingTempPoint = 1;
                    return;
                }
                if (args.Parameters[1] == "2")
                {
                    player.SendInfoMessage("Select point 2");
                    player.AwaitingTempPoint = 2;
                    return;
                }
            }
            #endregion

            #region stats
            if (args.Parameters[0].ToLower() == "stats")
            {
                if (args.Parameters.Count == 1)
                {
                    if (selectedArena == "none")
                    {
                        player.SendErrorMessage("Please select an arena or specify one by using /arena stats [arena name]");
                        return;
                    }
                }
                else
                {
                    int resultType = -100;
                    args.Parameters.RemoveAt(0);
                    string search = string.Join(" ", args.Parameters);
                    string results = AUtil.ArenaExists(search, ref resultType);
                    switch (resultType)
                    {
                        case -1:
                            player.SendErrorMessage(results);
                            return;
                        case 0:
                        case 1:
                            a = AUtil.GetArena(results);
                            break;
                        case 2:
                            player.SendErrorMessage("Multiple arenas found: ");
                            player.SendErrorMessage(results);
                            return;
                    }

                }
                player.SendInfoMessage("Stats for " + a.Name + ".");
                player.SendInfoMessage("Wins: " + a.Stats.Wins());
                player.SendInfoMessage("Game lengths: Average: " + ((a.Stats.GameLength / 10) / a.Stats.totalGames()) + " seconds, Cumulative: " + a.Stats.GameLength / 10 + " seconds.");
                player.SendInfoMessage("Deaths: " + a.Stats.TotalDeaths + " total, with " + a.Stats.MapKills + " caused by the environment. Suckers.");
                return;
            }

            #endregion

            #region script
            if (args.Parameters[0].ToLower() == "script")
            {
                player.SendInfoMessage("Haha, get baited nerd. This shit ain't done yet, because coding it sucks.");
                return;
            }
            #endregion
        }

        private void ClassMod(CommandArgs args)
        {
            TSPlayer player = args.Player;
            #region help
            if (args.Parameters.Count == 0 || args.Parameters[0] == "help")
            {
                player.SendErrorMessage("Aliases: /cwmod, /cwm");
                player.SendErrorMessage("/cwm select [class name]");
                player.SendErrorMessage("/cwm add [class name]");
                player.SendErrorMessage("/cwm del");
                player.SendErrorMessage("/cwm set {inv | stats}");
                player.SendErrorMessage("/cwm buff {add | del | clear} [buff name/id] [duration] <itemHeld>");
                player.SendErrorMessage("/cwm itemgen {add | del | clear} [refresh] [maxQuantity]");
                player.SendErrorMessage("/cwm link {set | clear}");
                player.SendErrorMessage("/cwm desc {get | set} [line] [description text]");
                player.SendErrorMessage("/cwm category {get | set | list} [category]");
                return;
            }
            #endregion

            /*wheeee, first command is done, on to the other three. But first, have some command syntax before i forget to write this down anywhere outside of the source code.*/

            #region select
            if (args.Parameters[0] == "select")
            {
                if (args.Parameters.Count == 1)
                {
                    player.SendErrorMessage("Usage: /cwm select [class name]");
                    player.SendErrorMessage("This selects a class for other class modifying commands to reference");
                    return;
                }
                int resultType = 0;
                args.Parameters.RemoveAt(0);
                string search = string.Join(" ", args.Parameters);
                string results = CUtil.ClassExists(search, ref resultType);
                switch (resultType)
                {
                    case -1:
                        player.SendErrorMessage(results);
                        break;
                    case 0:
                    case 1:
                        PInfo.SelectClass(player.Name, results);
                        player.SendSuccessMessage("Class " + results + " selected.");
                        break;
                    case 2:
                        player.SendErrorMessage("Multiple classes found: ");
                        player.SendErrorMessage(results);
                        break;
                }
                return;
            }
            #endregion

            #region add
            if (args.Parameters[0] == "add")
            {
                if (args.Parameters.Count == 0)
                {
                    player.SendErrorMessage("Usage: /cwm add [class name]");
                    player.SendErrorMessage("This adds a new class with the given name, using your current inventory and stats");
                    return;
                }
                int resultType = 0;
                args.Parameters.RemoveAt(0);
                string name = string.Join(" ", args.Parameters);
                string results = CUtil.ClassExists(name, ref resultType);
                if (resultType == 0)
                {
                    player.SendErrorMessage("There is already a class named " + name);
                    return;
                }
                CUtil.AddClass(player, name);
                player.SendSuccessMessage("Class " + name + " successfully added.");
                PInfo.SelectClass(player.Name, name);
                player.SendSuccessMessage("Class " + name + " has been automatically selected.");
                return;
            }
            #endregion

            #region checkSelected
            string selectedClass = PInfo.GetClassEdit(player.Name);
            if (selectedClass == "none")
            {
                player.SendErrorMessage("Please select a class using \"/cwm select\"before using this command.");
                return;
            }
            CWClass c = CUtil.GetClass(selectedClass);
            #endregion

            #region del
            if (args.Parameters[0].ToLower() == "del")
            {
                if (CUtil.DelClass(c.name))
                {
                    player.SendSuccessMessage("Class " + c.name + " successfully deleted.");
                }
                else
                {
                    player.SendErrorMessage("Warning: Unknown class selected: " + c.name);
                    player.SendErrorMessage("Deselecting unknown class.");
                }
                PInfo.SelectClass(player.Name, "none");
                return;
            }
            #endregion

            #region set
            if (args.Parameters[0].ToLower() == "set")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[1].ToLower() != "inv" && args.Parameters[1].ToLower() != "stats" && args.Parameters[1].ToLower() != "stat"))
                {
                    player.SendErrorMessage("Usage: /cwm set {inv | stats}");
                    player.SendErrorMessage("This sets the inventory or health/mana of a class to that of your current player.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "inv")
                {
                    c.inventory = player.PlayerData.inventory;
                }
                if (args.Parameters[1].ToLower() == "stat" || args.Parameters[1].ToLower() == "stats")
                {
                    c.maxHealth = player.PlayerData.maxHealth;
                    c.maxMana = player.PlayerData.maxMana;
                    c.extraSlot = player.PlayerData.extraSlot;
                }
                CUtil.UpdateClass(c);
                player.SendSuccessMessage("Successfully updated " + c.name);
                return;
            }
            #endregion

            #region buff
            if (args.Parameters[0].ToLower() == "buff")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[1].ToLower() != "del" && args.Parameters[1].ToLower() != "add" && args.Parameters[1].ToLower() != "clear"))
                {
                    player.SendErrorMessage("Usage: /cwm buff {add | del | clear} [buff name/id] [duration] <itemHeld>");
                    player.SendErrorMessage("This allows modifications of a class's buffs and itembuffs.");
                    player.SendErrorMessage("<itemHeld> should be 0 or empty for permanent buffs, 1 for buffs while holding an item, and 2 for buffs while not holding an item.");
                    player.SendErrorMessage("if <itemheld> is not 0, the currently item you are currently holding is used.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "clear")
                {
                    c.buffs = new List<Buff>();
                    CUtil.UpdateClass(c);
                    player.SendSuccessMessage("All buffs cleared from class " + c.name);
                    return;
                }
                if (args.Parameters[1].ToLower() == "add")
                {
                    if (args.Parameters.Count < 4)
                    {
                        player.SendErrorMessage("Usage: /cwm buff add [buff name/id] [duration] <itemHeld>");
                        player.SendErrorMessage("<itemHeld> should be 0 or empty for permanent buffs, 1 for buffs while holding an item, and 2 for buffs while not holding an item.");
                        player.SendErrorMessage("if <itemheld> is not 0, the currently item you are currently holding is used.");
                        return;
                    }
                    int buffID;
                    if (!int.TryParse(args.Parameters[2].ToLower(), out buffID))
                    {
                        List<int> t = TShock.Utils.GetBuffByName(args.Parameters[2].ToLower());
                        if (t.Count > 1)
                        {
                            player.SendErrorMessage("Multiple buff IDs found: " + string.Join(", ", t));
                            return;
                        }
                        else if (t.Count == 1)
                        {
                            buffID = t[0];
                        }
                        else if (t.Count == 0)
                        {
                            player.SendErrorMessage("Buff " + args.Parameters[2] + " not found.");
                            return;
                        }
                    }
                    float duration;
                    if (!float.TryParse(args.Parameters[3].ToLower(), out duration))
                    {
                        player.SendErrorMessage("Please only use numerical durations.");
                        return;
                    }
                    if (args.Parameters.Count == 5 && args.Parameters[4].ToLower() != "0" && args.Parameters[4].ToLower() != "no")
                    {
                        Item tempItem = player.TPlayer.HeldItem;
                        switch (args.Parameters[4].ToLower())
                        {
                            case "1":
                                c.buffs.Add(new Buff(buffID, (int)duration * 10, tempItem.netID, 1));
                                CUtil.UpdateClass(c);
                                player.SendSuccessMessage("Buff " + TShock.Utils.GetBuffName(buffID) + " has been added to " + c.name + " with a duration of " + duration + " seconds, while holding " + tempItem.Name);
                                return;
                            case "2":
                                c.buffs.Add(new Buff(buffID, (int)duration * 10, tempItem.netID, 0));
                                CUtil.UpdateClass(c);
                                player.SendSuccessMessage("Buff " + TShock.Utils.GetBuffName(buffID) + " has been added to " + c.name + " with a duration of " + duration + " seconds, while not holding " + tempItem.Name);
                                return;
                        }
                    }
                    c.buffs.Add(new Buff(buffID, (int)duration * 10));
                    CUtil.UpdateClass(c);
                    player.SendSuccessMessage("Buff " + TShock.Utils.GetBuffName(buffID) + " has been added to " + c.name + " with a duration of " + duration + " seconds.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "del")
                {
                    int buffID;
                    if (!int.TryParse(args.Parameters[2].ToLower(), out buffID))
                    {
                        List<int> t = TShock.Utils.GetBuffByName(args.Parameters[2].ToLower());
                        if (t.Count > 1)
                        {
                            player.SendErrorMessage("Multiple buff IDs found: " + string.Join(", ", t));
                            return;
                        }
                        else if (t.Count == 1)
                        {
                            buffID = t[0];
                        }
                        else if (t.Count == 0)
                        {
                            player.SendErrorMessage("Buff " + args.Parameters[2] + " not found.");
                            return;
                        }
                    }
                    c.buffs.RemoveAll(b => b.id == buffID);
                    CUtil.UpdateClass(c);
                    player.SendSuccessMessage("All instances of " + TShock.Utils.GetBuffName(buffID) + " have been removed from " + c.name + ".");
                    return;
                }
                return;
            }
            #endregion

            #region itemgen
            if (args.Parameters[0].ToLower() == "itemGen")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[1].ToLower() != "del" && args.Parameters[1].ToLower() != "add" && args.Parameters[1].ToLower() != "clear"))
                {
                    player.SendErrorMessage("Usage: /cwm itemgen {add | del | clear} [refresh] [maxQuantity]");
                    player.SendErrorMessage("This allows modification of a class's regenerating items.");
                    player.SendErrorMessage("'/cwm itemgen add' utilizes the prefix, quantity, and item id of the player's currently selected item.");
                    player.SendErrorMessage("The stack size of the currently held item determines the number of items given at one time.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "clear")
                {
                    c.items = new List<ItemRestock>();
                    CUtil.UpdateClass(c);
                    player.SendSuccessMessage("Class " + c.name + " has been cleared of itemgens.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "add")
                {
                    if (args.Parameters.Count < 4)
                    {
                        player.SendErrorMessage("Usage: /cwm itemgen add [refresh] [maxQuantity]");
                        return;
                    }
                    float refresh;
                    if (!float.TryParse(args.Parameters[2], out refresh))
                    {
                        player.SendErrorMessage("Please use numerals for the refresh time.");
                        return;
                    }
                    int maxCount;
                    if (!int.TryParse(args.Parameters[3], out maxCount))
                    {
                        player.SendErrorMessage("Please use numerals for the maximum item count.");
                        return;
                    }
                    Item tempItem = player.TPlayer.HeldItem;
                    c.items.Add(new ItemRestock((int)refresh * 10, tempItem.netID, tempItem.stack, maxCount, tempItem.prefix));
                    CUtil.UpdateClass(c);
                    if (tempItem.prefix == 0)
                        player.SendSuccessMessage(c.name + " will now receive " + tempItem.stack + " " + tempItem.Name + " every " + (float)refresh / 10 + " seconds.");
                    else
                        player.SendSuccessMessage(c.name + " will now receive " + tempItem.stack + " " + TShock.Utils.GetPrefixById(tempItem.prefix) + " " + tempItem.Name + " every " + (float)refresh / 10 + " seconds.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "del")
                {
                    Item tempItem = player.TPlayer.HeldItem;
                    player.SendSuccessMessage(c.items.RemoveAll(i => i.item == tempItem.netID) + " itemgen(s) removed from " + c.name + ".");
                    CUtil.UpdateClass(c);
                    return;
                }
                return;
            }
            #endregion

            #region link
            if (args.Parameters[0].ToLower() == "link")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[1].ToLower() != "set" && args.Parameters[1].ToLower() != "clear"))
                {
                    player.SendErrorMessage("Usage: /cwm link {set | clear}");
                    player.SendErrorMessage("This command allows you set and remove locations that will cause a player to automatically preview a class.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "clear")
                {
                    c.locations = new List<LocationLink>();
                    CUtil.UpdateClass(c);
                    player.SendSuccessMessage("All location links removed from " + c.name);
                    return;
                }
                if (args.Parameters[1].ToLower() == "set")
                {
                    c.locations.Add(new LocationLink(player.TileX, player.TileY));
                    player.SendSuccessMessage("New location link set at your current location for " + c.name);
                    CUtil.UpdateClass(c);
                    return;
                }
            }
            #endregion

            #region description
            if (args.Parameters[0].ToLower() == "desc" || args.Parameters[0].ToLower() == "description")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[1].ToLower() != "get" && args.Parameters[1].ToLower() != "set"))
                {
                    player.SendErrorMessage("Usage: /cwm desc {get | set} [line] [description text]");
                    player.SendErrorMessage("This command allows you to check and set the description of a class, one line at a time.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "get")
                {
                    if (c.description.Count == 0)
                    {
                        player.SendErrorMessage(c.name + " does not have a description.");
                        return;
                    }
                    foreach(string s in c.description)
                    {
                        player.SendSuccessMessage(s);
                    }
                    return;
                }
                if (args.Parameters[1].ToLower() == "set")
                {
                    if (args.Parameters.Count < 4)
                    {
                        player.SendErrorMessage("Usage: /cwm desc set [line] [description text]");
                        return;
                    }
                    int line;
                    if (!int.TryParse(args.Parameters[2], out line))
                    {
                        player.SendErrorMessage("Unable to parse line number.");
                        return;
                    }
                    args.Parameters.RemoveAt(0);
                    args.Parameters.RemoveAt(0);
                    args.Parameters.RemoveAt(0);
                    c.description[line - 1] = string.Join(" ", args.Parameters);
                    player.SendSuccessMessage("Description line " + line + " of " + c.name + " set.");
                    player.SendSuccessMessage("Current description for " + c.name + " is now:");
                    foreach (string s in c.description)
                    {
                        player.SendSuccessMessage(s);
                    }
                    return;
                }
            }
            #endregion

            #region category
            if (args.Parameters[0] == "cat" || args.Parameters[0] == "category")
            {
                if (args.Parameters.Count == 1 || (args.Parameters[1].ToLower() != "get" && args.Parameters[1].ToLower() != "set" && args.Parameters[1].ToLower() != "list"))
                {
                    player.SendErrorMessage("Usage: /cwm category {get | set | list} [category]");
                    player.SendErrorMessage("This command allows you to retrieve or specify the category of a class.");
                    player.SendErrorMessage("Categories should be a single word, and cannot consist entirely of numbers.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "get")
                {
                    if (c.category == "none")
                        player.SendErrorMessage(c.name + " does not have a category yet.");
                    else
                        player.SendSuccessMessage(c.name + " is in the \"" + c.category + "\" category.");
                    return;
                }
                if (args.Parameters[1].ToLower() == "set")
                {
                    if (args.Parameters.Count == 2)
                    {
                        player.SendErrorMessage("Usage: /cwm category set [category]");
                        player.SendErrorMessage("This specifies the category of the currently selected class.");
                        return;
                    }
                    float YouMotherFucker;
                    if (float.TryParse(args.Parameters[2], out YouMotherFucker))
                    {
                        player.SendErrorMessage("Didn't you read the help text? What kind of category is " + args.Parameters[2] + " anyways?");
                        TShockAPI.Commands.HandleCommand(TSPlayer.Server, "/slap " + player.Name + " 5");
                        return;
                    }
                    TextInfo TI = new CultureInfo("en-US", false).TextInfo;
                    c.category = TI.ToTitleCase(args.Parameters[2]);
                    CUtil.UpdateClass(c);
                    player.SendSuccessMessage(c.name + " is now categorized under \"" + c.category + "\"");
                    return;
                }
                if (args.Parameters[1].ToLower() == "list")
                {
                    List<string> categories = new List<string>();
                    foreach (CWClass x in CUtil.classes)
                    {
                        categories.Add(x.category);
                    }
                    categories = categories.Distinct().ToList();
                    player.SendSuccessMessage("Categories: ");
                    player.SendSuccessMessage(string.Join(", ", categories));
                    return;
                }

            }

            #endregion
        }

        private void ClassWars(CommandArgs args)
        {
            TSPlayer player = args.Player;
            #region help
            if (args.Parameters.Count == 0 || args.Parameters[0] == "help")
            {
                player.SendErrorMessage("Aliases: /classwars, /cw");
                player.SendErrorMessage("/cw start <arena>");
                player.SendErrorMessage("/cw join {red | blue}");
                player.SendErrorMessage("/cw stop");
                player.SendErrorMessage("/cw ready");
                player.SendErrorMessage("/cw status");
                player.SendErrorMessage("/cw random {map | miner | teams} <{red | blue}>");
                return;
            }
            #endregion

            #region start
            if (args.Parameters[0].ToLower() == "start")
            {
                if (!CWGame.TeamsExist())
                {
                    player.SendErrorMessage("Must have at least one player on each team.");
                    return;
                }
                if (CWGame.TeamsBalanced() == "good" || args.Parameters[1] == "force")
                {
                    CWGame.StartGame();
                }
                else
                {
                    player.SendErrorMessage(CWGame.TeamsBalanced());
                }
                return;
            }
            #endregion

            #region join
            if (args.Parameters[0].ToLower() == "join")
            {

            }
            #endregion

            #region stop
            if (args.Parameters[0].ToLower() == "stop")
            {

            }
            #endregion

            #region ready
            if (args.Parameters[0].ToLower() == "ready")
            {

            }
            #endregion

            #region status
            if (args.Parameters[0].ToLower() == "status")
            {

            }
            #endregion

            #region random
            if (args.Parameters[0].ToLower() == "random" || args.Parameters[0].ToLower() == "rand")
            {

            }
            #endregion
        }

        private void ClassAccess(CommandArgs args)
        {
            TSPlayer player = args.Player;
            #region help
            if (args.Parameters.Count == 0 || args.Parameters[0] == "help")
            {
                player.SendErrorMessage("Aliases: /class, /cs");
                player.SendErrorMessage("/class list <category>");
                player.SendErrorMessage("/class select <class name>");
                player.SendErrorMessage("/class preview <class name>");
                player.SendErrorMessage("/class random <category>");
                player.SendErrorMessage("/class stop");
                player.SendErrorMessage("If a class name is not specified, this command will attempt to use the most recently used class from /cwm select.");
                player.SendErrorMessage("/class stop will attempt to restore your inventory and set you to not playing or previewing any class.");
                return;
            }
            #endregion

            #region list
            if (args.Parameters[0].ToLower() == "list")
            {
                int page = 1;
                bool categoryExists = false;
                string category;
                List<string> results = new List<string>();
                PaginationTools.Settings settings = new PaginationTools.Settings
                {
                    HeaderFormat = "Class Wars Classes: ",
                    FooterFormat = "type /class list {{0}}",
                    NothingToDisplayString = "There are no classes presently defined."
                };
                if (args.Parameters.Count > 1)
                {

                    if (!int.TryParse(args.Parameters[1], out page))
                    {
                        categoryExists = true;
                        category = args.Parameters[1];
                        TextInfo TI = new CultureInfo("en-US", false).TextInfo;
                        category = TI.ToTitleCase(category); //sets to title case for minor improvement to pagination appearance later.
                        foreach (CWClass c in CUtil.classes)
                        {

                            if (c.category.ToLower() == category.ToLower())
                                results.Add(c.name);
                        }

                        settings = new PaginationTools.Settings
                        {
                            HeaderFormat = category + " Classes: ",
                            FooterFormat = "type /class list " + category + " {{0}}",
                            NothingToDisplayString = "There are no classes in the " + category + " category."
                        };
                    }
                }
                if (!categoryExists)
                {
                    foreach (CWClass c in CUtil.classes)
                        results.Add(c.name);
                }
                results.Sort();
                PaginationTools.SendPage(player, page, PaginationTools.BuildLinesFromTerms(results), settings);
                return;
            }
            #endregion

            #region select
            if (args.Parameters[0].ToLower() == "select" || args.Parameters[0].ToLower() == "pick" || args.Parameters[0].ToLower() == "sel")
            {
                if (args.Parameters.Count == 1 && PInfo.GetClassEdit(player.Name) == "none")
                {
                    player.SendErrorMessage("Usage: /class select <class name>");
                    player.SendErrorMessage("Alternatively, select a class using /cwm select before using /class select.");
                    return;
                }
                args.Parameters.RemoveAt(0);
                string className = PInfo.GetClassEdit(player.Name);
                if (args.Parameters.Count > 0)
                {
                    string c = string.Join(" ", args.Parameters);
                    int resultType = -100;
                    string results = CUtil.ClassExists(c, ref resultType);
                    switch (resultType)
                    {
                        case -1:
                            player.SendErrorMessage(results);
                            return;
                        case 0:
                        case 1:
                            className = results;
                            break;
                        case 2:
                            player.SendErrorMessage("Multiple classes found: ");
                            player.SendErrorMessage(results);
                            return;
                    }
                }
                CUtil.SetClass(player, className, ref PInfo);
                player.SendSuccessMessage("You have selected " + className + ".");

                //game logic here, moron. Don't forget it.



            }
            #endregion

            #region preview
            if (args.Parameters[0].ToLower() == "select" || args.Parameters[0].ToLower() == "pick" || args.Parameters[0].ToLower() == "sel")
            {
                if (args.Parameters.Count == 1 && PInfo.GetClassEdit(player.Name) == "none")
                {
                    player.SendErrorMessage("Usage: /class select <class name>");
                    player.SendErrorMessage("Alternatively, select a class using /cwm select before using /class select.");
                    return;
                }
                args.Parameters.RemoveAt(0);
                string className = PInfo.GetClassEdit(player.Name);
                if (args.Parameters.Count > 0)
                {
                    string c = string.Join(" ", args.Parameters);
                    int resultType = -100;
                    string results = CUtil.ClassExists(c, ref resultType);
                    switch (resultType)
                    {
                        case -1:
                            player.SendErrorMessage(results);
                            return;
                        case 0:
                        case 1:
                            className = results;
                            break;
                        case 2:
                            player.SendErrorMessage("Multiple classes found: ");
                            player.SendErrorMessage(results);
                            return;
                    }
                }
                CUtil.SetClass(player, className, ref PInfo);
                PInfo.SetPreviewing(player.Name, true);
                player.SendSuccessMessage("You are now previewing " + className + ".");
            }
            #endregion

            #region stop
            if (args.Parameters[0].ToLower() == "stop")
            {
                PInfo.RestoreInv(player);
                PInfo.SetPlaying(player.Name, "none");
                PInfo.SetPreviewing(player.Name, false);
                CWGame.GTFO(player);
                player.SendSuccessMessage("Inventory restore attempted, you are no longer playing or previewing a class.");
                return;
            }
            #endregion
            
            #region random
            if (args.Parameters[0].ToLower() == "random")
            {
                string category = "none";
                List<CWClass> classesToPick = new List<CWClass>();
                if (args.Parameters.Count > 1)
                {
                    args.Parameters.RemoveAt(0);
                    category = string.Join(" ", args.Parameters);
                    foreach (CWClass c in CUtil.classes)
                    {

                        if (c.category.ToLower() == category.ToLower())
                            classesToPick.Add(c);
                    }
                    if (classesToPick.Count == 0)
                    {
                        player.SendErrorMessage("No classes found in category " + category);
                        return;
                    }
                }
                else
                {
                    classesToPick = CUtil.classes;
                }
                if (classesToPick.Count == 0)
                {
                    player.SendErrorMessage("No classes are presently defined.");
                    return;
                }
                Random rand = new Random();
                TShockAPI.Commands.HandleCommand(player, "/class select " + classesToPick[rand.Next(classesToPick.Count)].name);
                return;
            }
            #endregion
        }
    }
}