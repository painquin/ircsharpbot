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

            // End of MOTD - we only get this once, and it means things are going well.
            bot.Handlers["376"] = (IrcMessage msg) =>
            {
                bot.Join("#cddev");
                bot.Join("#fightclub");

            };

            bot.Handlers["JOIN"] = (IrcMessage msg) =>
            {
                string cname = msg.Params[0];

                // I am joining a channel
                if (msg.Prefix == bot.Nickname)
                {
                    Console.WriteLine("Joined {0}", msg.Params[0]);
                    Channels[msg.Params[0].ToLower()] = new Channel(msg.Params[0]);
                    bot.Action(msg.Params[0], "takes a bow");
                }
                else
                {
                    Channels[cname.ToLower()].AddUser(GetUserByName(msg.Params[0]));
                }

            };
            // NAMES
            bot.Handlers["353"] = (IrcMessage msg) =>
            {
                string me = msg.Params[0];
                string thing = msg.Params[1]; // this is an = ?
                string chan = msg.Params[2];
                string[] names = msg.Params[3].Split(' ');
                // I must be in the channel already

                Channel ch = Channels[chan.ToLower()];
                foreach (string name in names)
                {
                    ch.AddUser(GetUserByName(name));
                }
            };
            Random r = new Random();

            bot.Handlers["PRIVMSG"] = (IrcMessage msg) =>
            {

            };
            bot.Handlers["NICK"] = (IrcMessage msg) =>
            {
                if (msg.Prefix != bot.Nickname)
                {
                    User u = GetUserByName(msg.Prefix);
                    Users.Remove(u.Nickname.ToLower());
                    u.Nickname = msg.Params[0];
                    Users.Add(msg.Params[0].ToLower(), u);
                }
            };
            bot.Handlers["MODE"] = (IrcMessage msg) =>
            {
                if (msg.Params[1] == "+o " && msg.Params[2] == bot.Nickname)
                {
                    bot.Privmsg(msg.Params[0], "bwahaha.");
                }
            };

            bot.Connect("irc.afternet.org", 6667);
            Console.ReadLine();
            bot.Quit();
        }
    }
}
