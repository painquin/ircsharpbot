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
            
            Users.Add(bot.Nickname, bot);

            Func<String,User> GetUserByName = (name) => {
                User u;
                if (!Users.TryGetValue(name, out u)) {
                    u = new User(name);
                    Users.Add(name, u);
                }
                return u;
            };

            // MOTD
            bot.Handlers["372"] = (IrcMessage msg) => { };


            bot.Handlers["JOIN"] = (IrcMessage msg) =>
            {
                // I am joining a channel
                if (msg.from == bot.Nickname)
                {
                    Channels[msg.msg] = new Channel(msg.msg);
                }
                else
                {
                    Channels[msg.msg].AddUser(GetUserByName(msg.from));
                }

            };
            // NAMES
            bot.Handlers["353"] = (IrcMessage msg) =>
            {
                Match m = Regex.Match(msg.msg, @"= (?<channel>[#&]\S+) :(?<name>(\S+)\s?)+", RegexOptions.ExplicitCapture);
                string chan = m.Groups["channel"].Value;
                // I must be in the channel already
                Channel ch = Channels[chan];
                foreach (Capture name in m.Groups["name"].Captures)
                {
                    ch.AddUser(GetUserByName(name.Value));
                }
            };

            bot.Handlers["PRIVMSG"] = (IrcMessage msg) =>
            {
                // to me?
                if (msg.to.ToLower() == bot.Nickname.ToLower())
                {
                    bot.Privmsg(msg.from, "HI! I'M A BOT!");
                }
                else
                {
                    //bot.Privmsg(msg.to, "I'm a bot in a channel.");
                }
            };
            bot.Handlers["MODE"] = (IrcMessage msg) =>
            {
                if (msg.msg == "+o " + bot.Nickname)
                {
                    bot.Privmsg(msg.to, "bwahaha.");
                }
            };
            bot.Connect("irc.bluecherry.net", 6667);
            Console.ReadLine();
            bot.Quit();
        }
    }
}
