using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public delegate void ConnectionEvent(object sender, Client user);
    public delegate void DataReceivedEvent(Client sender, byte[] data);
}
