using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ircsharp
{
    class IrcMessage
    {
        public string Prefix { get; protected set; }
        public string Command { get; protected set; }
        public List<String> Params { get; protected set; }

        protected static Regex re = new Regex(@"^(:(?<prefix>\S+)\s+)?(?<command>([A-z]+|\d{3}))(\s+(:(?<params>.+$)|(?<params>\S+)))*", RegexOptions.ExplicitCapture);

        public IrcMessage(string line)
        {
            Match match = re.Match(line);
            if (match.Success)
            {
                Prefix = match.Groups["prefix"].Value;
                if (Prefix.Contains('!')) Prefix = Prefix.Substring(0, Prefix.IndexOf('!'));
                Command = match.Groups["command"].Value;
                Params = new List<string>();
                Params.AddRange(from p in match.Groups["params"].Captures.Cast<Capture>() select p.Value);
            }
        }
    }
}
