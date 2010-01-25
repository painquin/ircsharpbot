using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ircsharp
{
    class IrcSharpBot : User
    {

        public delegate void Handler(IrcMessage msg);
        public Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();
        

        protected TcpClient tcp { get; set; }

        public IrcSharpBot(string nick) : base(nick)
        {
            tcp = new TcpClient();
            Level = 100;

            Handlers["PING"] = (msg) => Pong(msg.Params[0]);

        }

        public void Connect(string host, int port) {

            tcp.BeginConnect(host, port, (res) =>
            {
                tcp.EndConnect(res);
                onConnect();
            }, null);
        }

        protected void RawSend(string msg)
        {
            tcp.Client.Send(ASCIIEncoding.ASCII.GetBytes(msg));
        }

        protected void RawSend(string fmt, params object[] args)
        {
            string msg = String.Format(fmt, args);
            RawSend(msg);
        }

        public void Nick(string newnickname)
        {
            RawSend("NICK {0}\r\n", newnickname);
        }

        public void User(string username, string hostname, string servername, string realname)
        {
            RawSend("USER {0} {1} {2} {3}\r\n", username, hostname, servername, realname);
        }

        public void Quit(string msg)
        {
            RawSend("QUIT :{0}\r\n", msg);
        }

        public void Quit()
        {
            RawSend("QUIT :Connection reset by pier.\r\n");
        }

        public void Join(params string[] channels)
        {
            RawSend("JOIN {0}\r\n", String.Join(",", channels));
        }

        public void Privmsg(string to, string msg)
        {
            RawSend("PRIVMSG {0} :{1}\r\n", to, msg);
        }

        public void Kick(string channel, string name)
        {
            RawSend("KICK {0} {1} pwnt\r\n", channel, name);
        }

        // wrong.
        public void Action(string channel, string action)
        {
            RawSend("PRIVMSG {0} :", channel);
            tcp.Client.Send(new byte[]{1});
            RawSend("ACTION {0}", action);
            tcp.Client.Send(new byte[] { 1, 13, 10 });
        }


        private void onLine(string line)
        {
            bool found = false;
            
            IrcMessage message = new IrcMessage(line);

            Handler h;
            if (Handlers.TryGetValue(message.Command, out h))
            {
                h.Invoke(message);
                found = true;
            }
            
            if (!found)
            {
                Console.WriteLine(line);
            }
        }


        List<byte> incoming_buffer = new List<byte>();
        private void onReceive(IAsyncResult res) {
            int n = tcp.Client.EndReceive(res);
            byte[] bytes = res.AsyncState as byte[];
            incoming_buffer.AddRange(bytes.Take(n));
            int p = incoming_buffer.IndexOf(13);
            while (p != -1)
            {
                string line = ASCIIEncoding.ASCII.GetString(incoming_buffer.Take(p).ToArray());
                incoming_buffer.RemoveRange(0, p + 2);
                onLine(line);
                p = incoming_buffer.IndexOf(13);
            }

            tcp.Client.BeginReceive(bytes, 0, 1024, SocketFlags.None, onReceive, bytes);
        }


        private void onConnect()
        {
            byte[] buffer = new byte[1024];
            tcp.Client.BeginReceive(buffer, 0, 1024, SocketFlags.None, onReceive, buffer);
            User("sharpbot", "quin.sniqe.com", "irc.afternet.org", "coads");
            Nick(Nickname);
        }

        public void Pong(string server)
        {
            RawSend("PONG {0}\r\n", server);
        }
    }
}
