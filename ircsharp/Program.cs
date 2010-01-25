using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ircsharp
{
    class Program
    {



        static void Main(string[] args)
        {

            IrcSharpBot bot = new IrcSharpBot("CombatDrone");

            Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();
            Dictionary<string, User> Users = new Dictionary<string, User>();
            
            Users.Add(bot.Nickname.ToLower(), bot);

            Func<String,User> GetUserByName = (name) => {
                User u;
                if (!Users.TryGetValue(name.ToLower(), out u)) {
                    u = new User(name);
                    Users.Add(name.ToLower(), u);
                }
                return u;
            };

            // MOTD
            bot.Handlers["372"] = (IrcMessage msg) => { };


            // End of MOTD - we only get this once, and it means things are going well.
            bot.Handlers["376"] = (IrcMessage msg) =>
            {
                bot.Join("#cddev");
                bot.Join("#fightclub");

            };

            bot.Handlers["JOIN"] = (IrcMessage msg) =>
            {
                string cname = msg.msg;
                if (msg.msg == "") cname = msg.to;

                // I am joining a channel
                if (msg.from == bot.Nickname)
                {
                    Console.WriteLine("Joined {0}", cname);
                    Channels[cname.ToLower()] = new Channel(cname);
                    bot.Action(cname, "takes a bow");
                }
                else
                {
                    Channels[cname].AddUser(GetUserByName(msg.from));
                }

            };
            // NAMES
            bot.Handlers["353"] = (IrcMessage msg) =>
            {
                Match m = Regex.Match(msg.msg, @"[@=] (?<channel>[#&]\S+) :([@]?(?<name>\S+)\s?)+", RegexOptions.ExplicitCapture);
                string chan = m.Groups["channel"].Value;
                // I must be in the channel already
                Channel ch = Channels[chan.ToLower()];
                foreach (Capture name in m.Groups["name"].Captures)
                {
                    ch.AddUser(GetUserByName(name.Value));
                }
            };
            Random r = new Random();

            bot.Handlers["PRIVMSG"] = (IrcMessage msg) =>
            {
                // to me?
                if (msg.to.ToLower() == bot.Nickname.ToLower())
                {
                    bot.Privmsg(msg.from, "HI! I'M A BOT!");
                }
                else
                {
                    Match m = Regex.Match(msg.msg, @"(?<cmd>\S+)( ((?<arg>\S+)\s?)+)?", RegexOptions.ExplicitCapture);
                    if (m.Success)
                    {
                        string cmd = m.Groups["cmd"].Value;
                        string[] cmdargs = m.Groups["arg"].Captures.Cast<Capture>().Select(c => c.Value).ToArray();

                        Channel ch = null;
                        Channels.TryGetValue(msg.to.ToLower(), out ch);

                        switch (cmd)
                        {
                            case "attack":
                                User attacker, defender;

                                if (Users.TryGetValue(msg.from.ToLower(), out attacker) && cmdargs.Length > 0 && Users.TryGetValue(cmdargs[0].ToLower(), out defender))
                                {

                                    if (attacker == defender)
                                    {
                                        bot.Privmsg(ch.Name, string.Format("{0}: Stop hitting yourself.", attacker));
                                        break;
                                    }

                                    if (attacker.Level > defender.Level + 20)
                                    {
                                        bot.Privmsg(ch.Name, string.Format("{0}: That wouldn't be fair.", attacker));
                                        break;
                                    }
                                    else if (defender.Level > attacker.Level + 20)
                                    {
                                        bot.Privmsg(ch.Name, string.Format("{0}: That's probably not a good idea.", attacker));
                                        break;
                                    }

                                    int attacker_roll = r.Next(20) + 1 + attacker.Level;
                                    int defender_roll = r.Next(20) + 1 + defender.Level;

                                    int adj;

                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendFormat("{0} attacks {1}! ", attacker, defender);

                                    // attacker wins
                                    if (attacker_roll > defender_roll)
                                    {
                                        sb.AppendFormat("{0} wins! ({1} vs {2}) ", attacker, attacker_roll, defender_roll);
                                        if (attacker.Level > defender.Level)
                                        {
                                            adj = attacker.GainXP(defender.Level);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", attacker, attacker.Level);
                                            adj = defender.GainXP(attacker.Level / 5);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", attacker, attacker.Level);

                                        }
                                        else
                                        {
                                            adj = attacker.GainXP(defender.Level * 2);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", attacker.Nickname, attacker.Level);

                                            adj = defender.GainXP(attacker.Level / 10);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", attacker.Nickname, attacker.Level);

                                        }
                                    }
                                    else // defender wins
                                    {
                                        sb.AppendFormat("{0} wins! ({1} vs {2}) ", defender, attacker_roll, defender_roll);

                                        if (attacker.Level > defender.Level)
                                        {
                                            adj = attacker.GainXP(defender.Level / 10);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", attacker.Nickname, attacker.Level);

                                            adj = defender.GainXP(attacker.Level * 2);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", defender.Nickname, defender.Level);

                                        }
                                        else
                                        {
                                            adj = attacker.GainXP(defender.Level / 5);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", attacker.Nickname, attacker.Level);

                                            adj = defender.GainXP(attacker.Level);
                                            if (adj > 0)
                                                sb.AppendFormat("{0} has levelled up! {0} is now level {1}!", defender.Nickname, defender.Level);

                                        }
                                    }
                                    
                                    bot.Privmsg(ch.Name, sb.ToString());

                                }
                                break;

                            case "stats":
                                User u;
                                if ((cmdargs.Length > 0 && Users.TryGetValue(cmdargs[0].ToLower(), out u)) || Users.TryGetValue(msg.from.ToLower(), out u))
                                {
                                    bot.Privmsg(ch.Name, string.Format("{0} is Level {1} with {2}/{3} XP", u, u.Level, u.XP, u.Level * 10));
                                }
                                break;

                        }
                    }
                }
            };
            bot.Handlers["NICK"] = (IrcMessage msg) =>
            {
                if (msg.from != bot.Nickname)
                {
                    User u = GetUserByName(msg.from);
                    Users.Remove(u.Nickname.ToLower());
                    u.Nickname = msg.msg;
                    Users.Add(msg.msg.ToLower(), u);
                }
            };
            bot.Handlers["MODE"] = (IrcMessage msg) =>
            {
                if (msg.msg == "+o " + bot.Nickname)
                {
                    bot.Privmsg(msg.to, "bwahaha.");
                }
            };

            bot.Connect("irc.afternet.org", 6667);
            Console.ReadLine();
            bot.Quit();
        }
    }
}
