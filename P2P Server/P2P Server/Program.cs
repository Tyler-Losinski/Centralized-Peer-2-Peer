using System;
using System.Net;
using System.Net.Sockets;

/*
 * @author Adam Hart
 * 
 * Modified from Zeeshan Aamir Khavas's Java application:
 * "Centralized P2P Application"
 */

namespace P2P_Server
{
    class Program
    {
        static void Main()
        {
            int listenPort;

            // Prompt user to enter port (hard-code a port?? check assignment instructions)
            Console.WriteLine("Illuminati Confirmed");
            Console.Write("Enter Port: ");
            while (!Int32.TryParse(Console.ReadLine(), out listenPort))
            {
                Console.Write("Port Invalid.  Enter Port: ");
            }

            try
            {
                // Set up server
                TcpListener listenSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), listenPort);
                listenSocket.Stop();
                listenSocket.Start();
                Console.WriteLine("\r\n::Server is active on the following address & port::");
                Console.WriteLine("Address: {0}:{1}", ((IPEndPoint) listenSocket.LocalEndpoint).Address,
                    ((IPEndPoint)listenSocket.LocalEndpoint).Port);
                CentralIndex index = new CentralIndex();
                Console.WriteLine("\n---@ Peer Activity @---");

                // Listen for new Peer connections
                while (true)
                {
                    if (listenSocket.Pending())
                    {
                        TcpClient newClient = listenSocket.AcceptTcpClient();
                        PeerManager pm = new PeerManager(newClient, index);
                        pm.Start();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error occured: {0}\r\n{1}", e.Message, e.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
