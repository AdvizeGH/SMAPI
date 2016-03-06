﻿using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Inheritance;
using StardewValley;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewModdingAPI.Events;

namespace TrainerMod
{
    public class TrainerMod : Mod
    {
        public override string Name
        {
            get { return "Trainer Mod"; }
        }

        public override string Authour
        {
            get { return "Zoryn Aaron"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Description
        {
            get { return "Registers several commands to use. Most commands are trainer-like in that they offer forms of cheating."; }
        }

        public static int frozenTime;
        public static bool infHealth, infStamina, infMoney, freezeTime;

        public override void Entry(params object[] objects)
        {
            RegisterCommands();
            GameEvents.UpdateTick += Events_UpdateTick;
        }

        static void Events_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.player == null)
                return;

            if (infHealth)
            {
                Game1.player.health = Game1.player.maxHealth;
            }
            if (infStamina)
            {
                Game1.player.stamina = Game1.player.MaxStamina;
            }
            if (infMoney)
            {
                Game1.player.money = 999999;
            }
            if (freezeTime)
            {
                Game1.timeOfDay = frozenTime;
            }
        }

        public static void RegisterCommands()
        {
            Command.RegisterCommand("types", "Lists all value types | types").CommandFired += types_CommandFired;

            Command.RegisterCommand("hide", "Hides the game form | hide").CommandFired += hide_CommandFired;
            Command.RegisterCommand("show", "Shows the game form | show").CommandFired += show_CommandFired;

            Command.RegisterCommand("save", "Saves the game? Doesn't seem to work. | save").CommandFired += save_CommandFired;
            Command.RegisterCommand("load", "Shows the load screen | load").CommandFired += load_CommandFired;

            Command.RegisterCommand("exit", "Closes the game | exit").CommandFired += exit_CommandFired;
            Command.RegisterCommand("stop", "Closes the game | stop").CommandFired += exit_CommandFired;

            Command.RegisterCommand("player_setname", "Sets the player's name | player_setname <object> <value>", new[] { "(player, pet, farm)<object> (String)<value> The target name" }).CommandFired += player_setName;
            Command.RegisterCommand("player_setmoney", "Sets the player's money | player_setmoney <value>|inf", new[] { "(Int32)<value> The target money" }).CommandFired += player_setMoney;
            Command.RegisterCommand("player_setstamina", "Sets the player's stamina | player_setstamina <value>|inf", new[] { "(Int32)<value> The target stamina" }).CommandFired += player_setStamina;
            Command.RegisterCommand("player_setmaxstamina", "Sets the player's max stamina | player_setmaxstamina <value>", new[] { "(Int32)<value> The target max stamina" }).CommandFired += player_setMaxStamina;
            Command.RegisterCommand("player_sethealth", "Sets the player's health | player_sethealth <value>|inf", new[] { "(Int32)<value> The target health" }).CommandFired += player_setHealth;
            Command.RegisterCommand("player_setmaxhealth", "Sets the player's max health | player_setmaxhealth <value>", new[] { "(Int32)<value> The target max health" }).CommandFired += player_setMaxHealth;
            Command.RegisterCommand("player_setimmunity", "Sets the player's immunity | player_setimmunity <value>", new[] { "(Int32)<value> The target immunity" }).CommandFired += player_setImmunity;

            Command.RegisterCommand("player_setlevel", "Sets the player's specified skill to the specified value | player_setlevel <skill> <value>", new[] { "(luck, mining, combat, farming, fishing, foraging)<skill> (1-10)<value> The target level" }).CommandFired += player_setLevel;
            Command.RegisterCommand("player_setspeed", "Sets the player's speed to the specified value?", new[] { "(Int32)<value> The target speed [0 is normal]" }).CommandFired += player_setSpeed;
            Command.RegisterCommand("player_changecolour", "Sets the player's colour of the specified object | player_changecolor <object> <colour>", new[] { "(hair, eyes, pants)<object> (r,g,b)<colour>" }).CommandFired += player_changeColour;
            Command.RegisterCommand("player_changestyle", "Sets the player's style of the specified object | player_changecolor <object> <value>", new[] { "(hair, shirt, skin, acc, shoe, swim, gender)<object> (Int32)<value>" }).CommandFired += player_changeStyle;

            Command.RegisterCommand("player_additem", "Gives the player an item | player_additem <item> [count] [quality]", new[] { "(Int32)<id> (Int32)[count] (Int32)[quality]" }).CommandFired += player_addItem;
            Command.RegisterCommand("player_addmelee", "Gives the player a melee item | player_addmelee <item>", new[] { "?<item>" }).CommandFired += player_addMelee;
            Command.RegisterCommand("player_addring", "Gives the player a ring | player_addring <item>", new[] { "?<item>" }).CommandFired += player_addRing;

            Command.RegisterCommand("out_items", "Outputs a list of items | out_items", new[] { "" }).CommandFired += out_items;
            Command.RegisterCommand("out_melee", "Outputs a list of melee weapons | out_melee", new[] { "" }).CommandFired += out_melee;
            Command.RegisterCommand("out_rings", "Outputs a list of rings | out_rings", new[] { "" }).CommandFired += out_rings;
            Command.RegisterCommand("newitem", "Outputs a list of melee weapons | out_melee", new[] { "" }).CommandFired += RegisterNewItem;

            Command.RegisterCommand("world_settime", "Sets the time to the specified value | world_settime <value>", new[] { "(Int32)<value> The target time [06:00 AM is 600]" }).CommandFired += world_setTime;
            Command.RegisterCommand("world_freezetime", "Freezes or thaws time | world_freezetime <value>", new[] { "(0 - 1)<value> Whether or not to freeze time. 0 is thawed, 1 is frozen" }).CommandFired += world_freezeTime;
            Command.RegisterCommand("world_setday", "Sets the day to the specified value | world_setday <value>", new[] { "(Int32)<value> The target day [1-28]" }).CommandFired += world_setDay;
            Command.RegisterCommand("world_setseason", "Sets the season to the specified value | world_setseason <value>", new[] { "(winter, spring, summer, fall)<value> The target season" }).CommandFired += world_setSeason;
            Command.RegisterCommand("world_downminelevel", "Goes down one mine level? | world_downminelevel", new[] { "" }).CommandFired += world_downMineLevel;
            Command.RegisterCommand("world_setminelevel", "Sets mine level? | world_setminelevel", new[] { "(Int32)<value> The target level" }).CommandFired += world_setMineLevel;
        }

        static void types_CommandFired(object sender, EventArgsCommand e)
        {
            Log.Verbose("[Int32: {0} - {1}], [Int64: {2} - {3}], [String: \"raw text\"], [Colour: r,g,b (EG: 128, 32, 255)]", Int32.MinValue, Int32.MaxValue, Int64.MinValue, Int64.MaxValue);
        }

        static void hide_CommandFired(object sender, EventArgsCommand e)
        {
            Program.StardewInvoke(() => { Program.StardewForm.Hide(); });
        }

        static void show_CommandFired(object sender, EventArgsCommand e)
        {
            Program.StardewInvoke(() => { Program.StardewForm.Show(); });
        }

        static void save_CommandFired(object sender, EventArgsCommand e)
        {
            StardewValley.SaveGame.Save();
        }

        static void load_CommandFired(object sender, EventArgsCommand e)
        {
            Game1.activeClickableMenu = new StardewValley.Menus.LoadGameMenu();
        }

        static void exit_CommandFired(object sender, EventArgsCommand e)
        {
            Program.gamePtr.Exit();
            Environment.Exit(0);
        }

        static void player_setName(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string obj = e.Command.CalledArgs[0];
                string[] objs = "player,pet,farm".Split(new[] { ',' });
                if (objs.Contains(obj))
                {
                    switch (obj)
                    {
                        case "player":
                            Game1.player.Name = e.Command.CalledArgs[1];
                            break;
                        case "pet":
                            Log.Error("Pets cannot currently be renamed.");
                            break;
                        case "farm":
                            Game1.player.farmName = e.Command.CalledArgs[1];
                            break;
                    }
                }
                else
                {
                    Log.LogObjectInvalid();
                }
            }
            else
            {
                Log.LogObjectValueNotSpecified();
            }
        }

        static void player_setMoney(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0] == "inf")
                {
                    infMoney = true;
                }
                else
                {
                    infMoney = false;
                    int ou = 0;
                    if (Int32.TryParse(e.Command.CalledArgs[0], out ou))
                    {
                        Game1.player.Money = ou;
                        Log.Verbose("Set {0}'s money to {1}", Game1.player.Name, Game1.player.Money);
                    }
                    else
                    {
                        Log.LogValueNotInt32();
                    }
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_setStamina(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0] == "inf")
                {
                    infStamina = true;
                }
                else
                {
                    infStamina = false;
                    int ou = 0;
                    if (Int32.TryParse(e.Command.CalledArgs[0], out ou))
                    {
                        Game1.player.Stamina = ou;
                        Log.Verbose("Set {0}'s stamina to {1}", Game1.player.Name, Game1.player.Stamina);
                    }
                    else
                    {
                        Log.LogValueNotInt32();
                    }
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_setMaxStamina(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                int ou = 0;
                if (Int32.TryParse(e.Command.CalledArgs[0], out ou))
                {
                    Game1.player.MaxStamina = ou;
                    Log.Verbose("Set {0}'s max stamina to {1}", Game1.player.Name, Game1.player.MaxStamina);
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_setLevel(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string skill = e.Command.CalledArgs[0];
                string[] skills = "luck,mining,combat,farming,fishing,foraging".Split(new[] { ',' });
                if (skills.Contains(skill))
                {
                    int ou = 0;
                    if (Int32.TryParse(e.Command.CalledArgs[1], out ou))
                    {
                        switch (skill)
                        {
                            case "luck":
                                Game1.player.LuckLevel = ou;
                                break;
                            case "mining":
                                Game1.player.MiningLevel = ou;
                                break;
                            case "combat":
                                Game1.player.CombatLevel = ou;
                                break;
                            case "farming":
                                Game1.player.FarmingLevel = ou;
                                break;
                            case "fishing":
                                Game1.player.FishingLevel = ou;
                                break;
                            case "foraging":
                                Game1.player.ForagingLevel = ou;
                                break;
                        }
                    }
                    else
                    {
                        Log.LogValueNotInt32();
                    }
                }
                else
                {
                    Log.Error("<skill> is invalid");
                }
            }
            else
            {
                Log.Error("<skill> and <value> must be specified");
            }
        }

        static void player_setSpeed(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    Game1.player.addedSpeed = e.Command.CalledArgs[0].AsInt32();
                    Log.Verbose("Set {0}'s added speed to {1}", Game1.player.Name, Game1.player.addedSpeed);
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_changeColour(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string obj = e.Command.CalledArgs[0];
                string[] objs = "hair,eyes,pants".Split(new[] { ',' });
                if (objs.Contains(obj))
                {
                    string[] cs = e.Command.CalledArgs[1].Split(new[] { ',' }, 3);
                    if (cs[0].IsInt32() && cs[1].IsInt32() && cs[2].IsInt32())
                    {
                        Color c = new Color(cs[0].AsInt32(), cs[1].AsInt32(), cs[2].AsInt32());
                        switch (obj)
                        {
                            case "hair":
                                Game1.player.hairstyleColor = c;
                                break;
                            case "eyes":
                                Game1.player.changeEyeColor(c);
                                break;
                            case "pants":
                                Game1.player.pantsColor = c;
                                break;
                        }
                    }
                    else
                    {
                        Log.Error("<colour> is invalid");
                    }
                }
                else
                {
                    Log.LogObjectInvalid();
                }
            }
            else
            {
                Log.Error("<object> and <colour> must be specified");
            }
        }

        static void player_changeStyle(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 1)
            {
                string obj = e.Command.CalledArgs[0];
                string[] objs = "hair,shirt,skin,acc,shoe,swim,gender".Split(new[] { ',' });
                if (objs.Contains(obj))
                {
                    if (e.Command.CalledArgs[1].IsInt32())
                    {
                        int i = e.Command.CalledArgs[1].AsInt32();
                        switch (obj)
                        {
                            case "hair":
                                Game1.player.changeHairStyle(i);
                                break;
                            case "shirt":
                                Game1.player.changeShirt(i);
                                break;
                            case "acc":
                                Game1.player.changeAccessory(i);
                                break;
                            case "skin":
                                Game1.player.changeSkinColor(i);
                                break;
                            case "shoe":
                                Game1.player.changeShoeColor(i);
                                break;
                            case "swim":
                                if (i == 0)
                                    Game1.player.changeOutOfSwimSuit();
                                else if (i == 1)
                                    Game1.player.changeIntoSwimsuit();
                                else
                                    Log.Error("<value> must be 0 or 1 for this <object>");
                                break;
                            case "gender":
                                if (i == 0)
                                    Game1.player.changeGender(true);
                                else if (i == 1)
                                    Game1.player.changeGender(false);
                                else
                                    Log.Error("<value> must be 0 or 1 for this <object>");
                                break;
                        }
                    }
                    else
                    {
                        Log.LogValueInvalid();
                    }
                }
                else
                {
                    Log.LogObjectInvalid();
                }
            }
            else
            {
                Log.LogObjectValueNotSpecified();
            }
        }

        static void world_freezeTime(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    if (e.Command.CalledArgs[0].AsInt32() == 0 || e.Command.CalledArgs[0].AsInt32() == 1)
                    {
                        freezeTime = e.Command.CalledArgs[0].AsInt32() == 1;
                        frozenTime = freezeTime ? Game1.timeOfDay : 0;
                        Log.Verbose("Time is now " + (freezeTime ? "frozen" : "thawed"));
                    }
                    else
                    {
                        Log.Error("<value> should be 0 or 1");
                    }
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void world_setTime(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    if (e.Command.CalledArgs[0].AsInt32() <= 2600 && e.Command.CalledArgs[0].AsInt32() >= 600)
                    {
                        Game1.timeOfDay = e.Command.CalledArgs[0].AsInt32();
                        frozenTime = freezeTime ? Game1.timeOfDay : 0;
                        Log.Verbose("Time set to: " + Game1.timeOfDay);
                    }
                    else
                    {
                        Log.Error("<value> should be between 600 and 2600 (06:00 AM - 02:00 AM [NEXT DAY])");
                    }
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void world_setDay(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    if (e.Command.CalledArgs[0].AsInt32() <= 28 && e.Command.CalledArgs[0].AsInt32() > 0)
                    {
                        Game1.dayOfMonth = e.Command.CalledArgs[0].AsInt32();
                    }
                    else
                    {
                        Log.Verbose("<value> must be between 1 and 28");
                    }
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void world_setSeason(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                string obj = e.Command.CalledArgs[0];
                string[] objs = "winter,spring,summer,fall".Split(new[] { ',' });
                if (objs.Contains(obj))
                {
                    Game1.currentSeason = obj;
                }
                else
                {
                    Log.LogValueInvalid();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_setHealth(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0] == "inf")
                {
                    infHealth = true;
                }
                else
                {
                    infHealth = false;
                    if (e.Command.CalledArgs[0].IsInt32())
                    {
                        Game1.player.health = e.Command.CalledArgs[0].AsInt32();
                    }
                    else
                    {
                        Log.LogValueNotInt32();
                    }
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_setMaxHealth(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    Game1.player.maxHealth = e.Command.CalledArgs[0].AsInt32();
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_setImmunity(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    Game1.player.immunity = e.Command.CalledArgs[0].AsInt32();
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void player_addItem(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    int count = 1;
                    int quality = 0;
                    if (e.Command.CalledArgs.Length > 1)
                    {
                        Console.WriteLine(e.Command.CalledArgs[1]);
                        if (e.Command.CalledArgs[1].IsInt32())
                        {
                            count = e.Command.CalledArgs[1].AsInt32();
                        }
                        else
                        {
                            Log.Error("[count] is invalid");
                            return;
                        }

                        if (e.Command.CalledArgs.Length > 2)
                        {
                            if (e.Command.CalledArgs[2].IsInt32())
                            {
                                quality = e.Command.CalledArgs[2].AsInt32();
                            }
                            else
                            {
                                Log.Error("[quality] is invalid");
                                return;
                            }

                        }
                    }

                    StardewValley.Object o = new StardewValley.Object(e.Command.CalledArgs[0].AsInt32(), count);
                    o.quality = quality;

                    Game1.player.addItemByMenuIfNecessary((Item)o);
                }
                else
                {
                    Log.Error("<item> is invalid");
                }
            }
            else
            {
                Log.LogObjectValueNotSpecified();
            }
        }

        static void player_addMelee(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {

                    MeleeWeapon toAdd = new MeleeWeapon(e.Command.CalledArgs[0].AsInt32());
                    Game1.player.addItemByMenuIfNecessary(toAdd);
                    Log.Verbose("Given {0} to {1}", toAdd.Name, Game1.player.Name);
                }
                else
                {
                    Log.Error("<item> is invalid");
                }
            }
            else
            {
                Log.LogObjectValueNotSpecified();
            }
        }

        static void player_addRing(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    
                    Ring toAdd = new Ring(e.Command.CalledArgs[0].AsInt32());
                    Game1.player.addItemByMenuIfNecessary(toAdd);
                    Log.Verbose("Given {0} to {1}", toAdd.Name, Game1.player.Name);
                }
                else
                {
                    Log.Error("<item> is invalid");
                }
            }
            else
            {
                Log.LogObjectValueNotSpecified();
            }
        }

        static void out_items(object sender, EventArgsCommand e)
        {
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    Item it = new StardewValley.Object(i, 1);
                    if (it.Name != "Error Item")
                        Console.WriteLine(i + "| " + it.Name);
                }
                catch
                {

                }
            }
        }

        static void out_melee(object sender, EventArgsCommand e)
        {
            Dictionary<int, string> d = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
            Console.Write("DATA\\WEAPONS: ");
            foreach (var v in d)
            {
                Console.WriteLine(v.Key + " | " + v.Value);
            }
        }

        static void out_rings(object sender, EventArgsCommand e)
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    Item it = new Ring(i);
                    if (it.Name != "Error Item")
                        Console.WriteLine(i + "| " + it.Name);
                }
                catch
                {

                }
            }
        }

        static void world_downMineLevel(object sender, EventArgsCommand e)
        {
            Game1.nextMineLevel();
        }

        static void world_setMineLevel(object sender, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                if (e.Command.CalledArgs[0].IsInt32())
                {
                    Game1.enterMine(true, e.Command.CalledArgs[0].AsInt32(), "");
                }
                else
                {
                    Log.LogValueNotInt32();
                }
            }
            else
            {
                Log.LogValueNotSpecified();
            }
        }

        static void blank_command(object sender, EventArgsCommand e) { }

        static void RegisterNewItem(object sender, EventArgsCommand e)
        {
#if DEBUG
            SObject s = SGame.PullModItemFromDict(0, true);
            s.Stack = 999;
            Game1.player.addItemToInventory(s);
#endif
        }
    }
}
