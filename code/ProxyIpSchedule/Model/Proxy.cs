using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyIpSchedule.Model
{
    public class Proxy
    {
        public string ip { get; set; }
        public int port { get; set; }

        public Proxy(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

    }
}
