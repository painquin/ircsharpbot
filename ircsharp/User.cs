using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ircsharp
{
    class User
    {
        public string Nickname { get; protected set; }

        public User(string nick)
        {
            Nickname = nick;
        }
    }
}
