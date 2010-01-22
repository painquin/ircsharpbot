using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ircsharp
{
    class IrcMessage
    {
        public string from { get; protected set; }
        public string msgtype { get; protected set; }
        public string to { get; protected set; }
        public string msg { get; protected set; }

        public IrcMessage(string line)
        {
            Match match = Regex.Match(line, @":(?<sender>\S+) (?<msgtype>\S+)( (?<recp>[^:\s]+))?( :?(?<msg>.+))?", RegexOptions.ExplicitCapture);

            from = match.Groups["sender"].Value;
            if (from.Contains('!'))
            {
                from = from.Substring(0, from.IndexOf('!'));
            }
            msgtype = match.Groups["msgtype"].Value;
            to = match.Groups["recp"].Value;
            msg = match.Groups.Count > 3 ? match.Groups["msg"].Value : "";
        }
    }
}
