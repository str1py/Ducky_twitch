using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    public class MessageType
    {
        public enum Type
        {
            ERROR,
            DEBUGINFO,
            RESUB,
            NEWSUB,
            SUBGIFT,
            ANONGIFT,
            STATS,
            EVENT
        }
    }
}
