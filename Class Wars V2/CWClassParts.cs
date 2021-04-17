using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Class_Wars_V2
{
    internal class Buff
    {
        internal int id, duration;
        internal int item;
        internal bool held;

        internal Buff(int id, int duration, int item, int held)
        {
            this.id = id;
            this.duration = duration;
            this.item = item;
            this.held = (held > 0);
        }

        internal Buff(int id, int duration)
        {
            this.id = id;
            this.duration = duration;
            item = -100;
            held = false;
        }
    }

    internal class ItemRestock
    {
        internal int refresh, item, quantity, maxCount, prefix;

        internal ItemRestock(int refresh, int item, int quantity, int maxCount, int prefix)
        {
            this.refresh = refresh;
            this.item = item;
            this.quantity = quantity;
            this.maxCount = maxCount;
            this.prefix = prefix;
        }
    }

    internal class Aura
    {
        internal int id, duration, range, targeting;    //Targeting: 1 for friendly, 2 for hostile, 3 for both
        internal Aura(int id, int duration, int range, int targeting)
        {
            this.id = id;
            this.duration = duration;
            this.range = range;
            this.targeting = targeting;
        }
    }

    internal class Stat
    {
        internal int kills, deaths, wins, losses, timePlayed;

        internal Stat(int kills, int deaths, int wins, int losses, int timePlayed)
        {
            this.kills = kills;
            this.deaths = deaths;
            this.wins = wins;
            this.losses = losses;
            this.timePlayed = timePlayed;
        }
    }

    internal class LocationLink
    {
        internal Vector2 location;
        internal LocationLink(float x, float y)
        {
            location.X = x;
            location.Y = y;
        }

        internal LocationLink(Vector2 loc)
        {
            location = loc;
        }
    }

    internal static class DeBlob
    {
        internal static List<Aura> Aura(string blob)
        {
            List<Aura> AuraList = new List<Aura>();
            string[] a = blob.Split('☺');
            string[] b;
            foreach (string s in a)
            {
                b = s.Split('◙');
                try
                {
                    AuraList.Add(new Aura(int.Parse(b[0]), int.Parse(b[1]), int.Parse(b[2]), int.Parse(b[3])));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Error loading Aura with id " + b[0] + " duration " + b[1] + " range " + b[2] + " targeting " + b[3]);
                    Console.WriteLine("Skipping invalid Aura and continuing");
                }
            }
            return AuraList;
        }

        internal static List<Buff> Buff(string blob)
        {
            List<Buff> BuffList = new List<Buff>();
            string[] a = blob.Split('☺');
            string[] b;
            foreach (string s in a)
            {
                b = s.Split('◙');
                try
                {
                    BuffList.Add(new Buff(int.Parse(b[0]), int.Parse(b[1]), int.Parse(b[2]), int.Parse(b[3])));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Error loading Buff with id " + b[0] + " duration " + b[1] + " item " + b[2] + " held " + b[3]);
                    Console.WriteLine("Skipping invalid Buff and continuing");
                }
            }
            return BuffList;
        }

        internal static List<ItemRestock> ItemRestock(string blob)
        {
            List<ItemRestock> ItemRestockList = new List<ItemRestock>();
            string[] a = blob.Split('☺');
            string[] b;
            foreach (string s in a)
            {
                b = s.Split('◙');
                try
                {
                    ItemRestockList.Add(new ItemRestock(int.Parse(b[0]), int.Parse(b[1]), int.Parse(b[2]), int.Parse(b[3]), int.Parse(b[4])));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Error loading ItemRestock with refresh " + b[0] + " item " + b[1] + " quantity " + b[2] + " maxCount " + b[3] + "prefix" + b[4]);
                    Console.WriteLine("Skipping invalid ItemRestock and continuing");
                }
            }
            return ItemRestockList;
        }

        internal static List<Stat> Stat(string blob)
        {
            List<Stat> StatList = new List<Stat>();
            string[] a = blob.Split('☺');
            string[] b;
            foreach (string s in a)
            {
                b = s.Split('◙');
                try
                {
                    StatList.Add(new Stat(int.Parse(b[0]), int.Parse(b[1]), int.Parse(b[2]), int.Parse(b[3]), int.Parse(b[4])));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Error loading Stat with kills " + b[0] + " deaths " + b[1] + " wins " + b[2] + " losses " + b[3] + "timePlayed" + b[4]);
                    Console.WriteLine("Skipping invalid Stat and continuing");
                }
            }
            return StatList;
        }

        internal static List<LocationLink> LocationLink(string blob)
        {
            List<LocationLink> LocationList = new List<LocationLink>();
            string[] a = blob.Split('☺');
            string[] b;
            foreach (string s in a)
            {
                b = s.Split('◙');
                try
                {
                    LocationList.Add(new LocationLink(float.Parse(b[0]), float.Parse(b[1])));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Error loading LocationLink with X " + b[0] + " Y " + b[1]);
                    Console.WriteLine("Skipping invalid LocationLink and continuing");
                }
            }
            return LocationList;
        }

        internal static List<string> Description(string blob)
        {
            return blob.Split('☺').ToList();
        }

        internal static NetItem[] Inventory(string blob)
        {
            return blob.Split('☺').Select(NetItem.Parse).ToArray();
        }
    }

    internal static class Blob
    {
        internal static string Aura(List<Aura> preblob)
        {
            List<string> blobules = new List<string>();
            foreach (Aura a in preblob)
            {
                blobules.Add(a.id + "◙" + a.duration + "◙" + a.range + "◙" + a.targeting);
            }
            string blob = string.Join("☺", blobules);
            return blob;
        }


        internal static string Buff(List<Buff> preblob)
        {
            List<string> blobules = new List<string>();
            foreach (Buff b in preblob)
            {
                blobules.Add(b.id + "◙" + b.duration + "◙" + b.item + "◙" + b.held);
            }
            string blob = string.Join("☺", blobules);
            return blob;
        }

        internal static string ItemRestock(List<ItemRestock> preblob)
        {
            List<string> blobules = new List<string>();
            foreach (ItemRestock i in preblob)
            {
                blobules.Add(i.refresh + "◙" + i.item + "◙" + i.quantity + "◙" + i.maxCount + "◙" + i.prefix);
            }
            string blob = string.Join("☺", blobules);
            return blob;
        }

        internal static string Stat(List<Stat> preblob)
        {
            List<string> blobules = new List<string>();
            foreach (Stat s in preblob)
            {
                blobules.Add(s.kills + "◙" + s.deaths + "◙" + s.wins + "◙" + s.losses + "◙" + s.timePlayed);
            }
            string blob = string.Join("☺", blobules);
            return blob;
        }

        internal static string LocationLinks(List<LocationLink> preblob)
        {
            List<string> blobules = new List<string>();
            foreach (LocationLink l in preblob)
            {
                blobules.Add(l.location.X + "◙" + l.location.Y);
            }
            string blob = string.Join("☺", blobules);
            return blob;
        }

        internal static string Description(List<string> preblob)
        {
            return string.Join("☺", preblob);
        }

        internal static string Inventory(NetItem[] preblob)
        {
            return string.Join("☺", preblob);
        }
    }
}
