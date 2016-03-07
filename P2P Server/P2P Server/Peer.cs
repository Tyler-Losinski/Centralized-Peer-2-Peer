using System;

/*
 * @author Adam Hart
 * 
 * Modified from Zeeshan Aamir Khavas's Java application:
 * "Centralized P2P Application"
 */

namespace P2P_Server
{
    public class Peer
    {
        public String IpAddress { get; set; }
        public int Port { get; set; }

        public String FullHost()
        {
            return IpAddress + ":" + Port;
        }

    }
}
