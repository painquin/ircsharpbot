using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ircsharp
{
    class User
    {
        public string Nickname { get; set; }

        public int Level { get; protected set; }
        public int XP { get; protected set; }

        public User(string nick)
        {
            Nickname = nick;
            Level = 1;
            XP = 0;
        }

        public int GainXP(int amt) {
            int oldlevel = Level;
            XP += amt;
            while(XP >= Level * 10) {
                XP -= Level * 10;
                Level += 1;
            }
            return Level - oldlevel;
        }

        public override string ToString()
        {
            return Nickname;
        }
    }
}
