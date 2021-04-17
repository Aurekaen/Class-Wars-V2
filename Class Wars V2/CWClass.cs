using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace Class_Wars_V2
{
    internal class CWClass
    {
        internal string name, category;
        internal NetItem[] inventory;
        internal List<string> description;
        internal int maxHealth, maxMana;
        internal int? extraSlot;
        internal List<Buff> buffs;
        internal List<ItemRestock> items;
        internal List<Aura> auras;
        internal List<Stat> stats;
        internal List<LocationLink> locations;

        internal CWClass(string name, string category, List<string> description, List<Buff> buffs, List<ItemRestock> items, List<Aura> auras, NetItem[] inventory, int maxHealth, int maxMana, int extraSlot, List<LocationLink> locations, List<Stat> stats)
        {
            this.name = name;
            this.category = category;
            this.description = description;
            this.buffs = buffs;
            this.items = items;
            this.auras = auras;
            this.inventory = inventory;
            this.maxHealth = maxHealth;
            this.maxMana = maxMana;
            this.extraSlot = extraSlot;
            this.locations = locations;
            this.stats = stats;
        }

        internal CWClass(string name, string category, string descriptionBlob, string buffBlob, string itemRestockBlob, string auraBlob, string inventoryBlob, int maxHealth, int maxMana, int extraSlot, string locationBlob, string statBlob)
        {
            this.name = name;
            this.category = category;
            description = DeBlob.Description(descriptionBlob);
            buffs = DeBlob.Buff(buffBlob);
            items = DeBlob.ItemRestock(itemRestockBlob);
            auras = DeBlob.Aura(auraBlob);
            inventory = DeBlob.Inventory(inventoryBlob);
            this.maxHealth = maxHealth;
            this.maxMana = maxMana;
            this.extraSlot = extraSlot;
            locations = DeBlob.LocationLink(locationBlob);
            stats = DeBlob.Stat(statBlob);
        }
    }

    internal class ClassUtils
    {
        internal List<CWClass> classes = new List<CWClass>();
        internal ClassDB class_db;

        internal ClassUtils()
        {
            class_db = ClassDB.InitDb("Classes");
            class_db.LoadClasses(ref classes);
        }

        internal string ClassExists(string lookup, ref int resultType)
        {
            lookup = lookup.ToLower();
            string results = "none";
            int resultCount = 0;
            foreach (CWClass c in classes)
            {
                if (c.name.ToLower() == lookup)
                {
                    resultType = 0;
                    return c.name;
                }
                if (c.name.ToLower().Contains(lookup))
                {
                    if (resultCount == 0)
                    {
                        results = c.name;
                    }
                    else
                    {
                        results = string.Join(", ", c.name);
                    }
                    resultCount++;
                }
            }
            switch (resultCount)
            {
                case 0:
                    resultType = -1;
                    results = "Class " + lookup + " was not found.";
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

        internal void AddClass(TSPlayer player, string name)
        {
            CWClass newClass = new CWClass(name, "none", new List<string>(), new List<Buff>(), new List<ItemRestock>(), new List<Aura>(), 
                player.PlayerData.inventory, player.PlayerData.maxHealth, player.PlayerData.maxMana, ExtraSlotUnNull(player.PlayerData.extraSlot), 
                new List<LocationLink>(), new List<Stat>());
            class_db.AddClass(newClass);
        }

        internal void UpdateClass(CWClass newClass)
        {
            classes[classes.FindIndex(c => c.name == newClass.name)] = newClass;
            class_db.UpdateClass(newClass);
        }

        internal CWClass GetClass(string name)
        {
            return classes[classes.FindIndex(c => c.name == name)];
        }

        internal bool DelClass(string name)
        {
            foreach(CWClass c in classes)
            {
                if (c.name == name)
                {
                    class_db.DeleteClass(c.name);
                    classes.Remove(c);
                    return true;
                }
            }
            return false;
        }

        internal int ExtraSlotUnNull(int? extraSlot)
        {
            int result = 0;
            if (extraSlot == null)
                extraSlot = -100;
            else
                result = (int)extraSlot;
            return result;

        }

        internal int? ExtraSlotReNull(int extraSlot)
        {
            int? result;
            if (extraSlot == -100)
                result = null;
            else
                result = extraSlot;
            return result;
        }

        internal void SetClass(TSPlayer player, string className, ref PlayerInfo PInfo)
        {
            if (PInfo.GetClassPlaying(player.Name) == "none")
            {
                PInfo.SetBackup(player);
            }
            PInfo.SetPlaying(player.Name, className);


            CWClass c = GetClass(className);
            player.IgnoreSSCPackets = true;
            player.PlayerData.health = c.maxHealth;
            player.PlayerData.maxHealth = c.maxHealth;
            player.TPlayer.statLifeMax2 = c.maxHealth;
            player.TPlayer.statLifeMax = c.maxHealth;
            player.TPlayer.statLife = c.maxHealth;
            player.TPlayer.statManaMax = c.maxMana;
            player.TPlayer.statMana = c.maxMana;
            if (c.extraSlot > 0)
                player.TPlayer.extraAccessory = c.extraSlot.Value == 1 ? true : false;


            //Fuck this shit down here, this is copied from tshock's playerdata.restore()
            #region invCopy
            for (int i = 0; i < NetItem.MaxInventory; i++)
            {
                if (i < NetItem.InventoryIndex.Item2)
                {
                    //0-58
                    player.TPlayer.inventory[i].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.inventory[i].netID != 0)
                    {
                        player.TPlayer.inventory[i].stack = c.inventory[i].Stack;
                        player.TPlayer.inventory[i].prefix = c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.ArmorIndex.Item2)
                {
                    //59-78
                    var index = i - NetItem.ArmorIndex.Item1;
                    player.TPlayer.armor[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.armor[index].netID != 0)
                    {
                        player.TPlayer.armor[index].stack = c.inventory[i].Stack;
                        player.TPlayer.armor[index].prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.DyeIndex.Item2)
                {
                    //79-88
                    var index = i - NetItem.DyeIndex.Item1;
                    player.TPlayer.dye[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.dye[index].netID != 0)
                    {
                        player.TPlayer.dye[index].stack = c.inventory[i].Stack;
                        player.TPlayer.dye[index].prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.MiscEquipIndex.Item2)
                {
                    //89-93
                    var index = i - NetItem.MiscEquipIndex.Item1;
                    player.TPlayer.miscEquips[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.miscEquips[index].netID != 0)
                    {
                        player.TPlayer.miscEquips[index].stack = c.inventory[i].Stack;
                        player.TPlayer.miscEquips[index].prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.MiscDyeIndex.Item2)
                {
                    //93-98
                    var index = i - NetItem.MiscDyeIndex.Item1;
                    player.TPlayer.miscDyes[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.miscDyes[index].netID != 0)
                    {
                        player.TPlayer.miscDyes[index].stack = c.inventory[i].Stack;
                        player.TPlayer.miscDyes[index].prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.PiggyIndex.Item2)
                {
                    //98-138
                    var index = i - NetItem.PiggyIndex.Item1;
                    player.TPlayer.bank.item[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.bank.item[index].netID != 0)
                    {
                        player.TPlayer.bank.item[index].stack = c.inventory[i].Stack;
                        player.TPlayer.bank.item[index].prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.SafeIndex.Item2)
                {
                    //138-178
                    var index = i - NetItem.SafeIndex.Item1;
                    player.TPlayer.bank2.item[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.bank2.item[index].netID != 0)
                    {
                        player.TPlayer.bank2.item[index].stack = c.inventory[i].Stack;
                        player.TPlayer.bank2.item[index].prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.TrashIndex.Item2)
                {
                    //179-219
                    var index = i - NetItem.TrashIndex.Item1;
                    player.TPlayer.trashItem.netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.trashItem.netID != 0)
                    {
                        player.TPlayer.trashItem.stack = c.inventory[i].Stack;
                        player.TPlayer.trashItem.prefix = (byte)c.inventory[i].PrefixId;
                    }
                }
                else if (i < NetItem.ForgeIndex.Item2)
                {
                    //220
                    var index = i - NetItem.ForgeIndex.Item1;
                    player.TPlayer.bank3.item[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.bank3.item[index].netID != 0)
                    {
                        player.TPlayer.bank3.item[index].stack = c.inventory[i].Stack;
                        player.TPlayer.bank3.item[index].Prefix((byte)c.inventory[i].PrefixId);
                    }
                }
                else
                {
                    //260
                    var index = i - NetItem.VoidIndex.Item1;
                    player.TPlayer.bank4.item[index].netDefaults(c.inventory[i].NetId);

                    if (player.TPlayer.bank4.item[index].netID != 0)
                    {
                        player.TPlayer.bank4.item[index].stack = c.inventory[i].Stack;
                        player.TPlayer.bank4.item[index].Prefix((byte)c.inventory[i].PrefixId);
                    }
                }
            }

            float slot = 0f;
            for (int k = 0; k < NetItem.InventorySlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, (float)Main.player[player.Index].inventory[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.ArmorSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, (float)Main.player[player.Index].armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.DyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, (float)Main.player[player.Index].dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscEquipSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, (float)Main.player[player.Index].miscEquips[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscDyeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, (float)Main.player[player.Index].miscDyes[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.PiggySlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.SafeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank2.item[k].prefix);
                slot++;
            }
            NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, (float)Main.player[player.Index].trashItem.prefix);
            for (int k = 0; k < NetItem.ForgeSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank3.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.VoidSlots; k++)
            {
                NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank4.item[k].prefix);
                slot++;
            }


            NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            slot = 0f;
            for (int k = 0; k < NetItem.InventorySlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, (float)Main.player[player.Index].inventory[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.ArmorSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, (float)Main.player[player.Index].armor[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.DyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, (float)Main.player[player.Index].dye[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscEquipSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, (float)Main.player[player.Index].miscEquips[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.MiscDyeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, (float)Main.player[player.Index].miscDyes[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.PiggySlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.SafeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank2.item[k].prefix);
                slot++;
            }
            NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, (float)Main.player[player.Index].trashItem.prefix);
            for (int k = 0; k < NetItem.ForgeSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank3.item[k].prefix);
                slot++;
            }
            for (int k = 0; k < NetItem.VoidSlots; k++)
            {
                NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank4.item[k].prefix);
                slot++;
            }



            NetMessage.SendData(4, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(42, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(16, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            for (int k = 0; k < 22; k++)
            {
                player.TPlayer.buffType[k] = 0;
            }
            #endregion
        }
    }
}
