using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ircsharp
{
    class Channel
    {
        public string Name { get; protected set; }
        public List<User> Users { get; protected set; }

        public Channel(string name)
        {
            Name = name;
            Users = new List<User>();
        }

        public void AddUser(User user)
        {
            if (Users.Contains(user)) return;
            Users.Add(user);
        }
    }
}
