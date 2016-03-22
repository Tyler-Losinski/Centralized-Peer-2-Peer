using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace P2PClient
{
    static class P2PManager
    {
        public static void StartListener(int listenPort)
        {
            Thread handler = new Thread(() => Listen(listenPort)) {Priority = ThreadPriority.AboveNormal};
            handler.Start();
        }
        
        private static void Listen(int listenPort)
        {
            // Set up server
            TcpListener listenSocket = new TcpListener(IPAddress.Any, listenPort);
            listenSocket.Stop();
            listenSocket.Start();
            Console.WriteLine("\r\n::Server is active on the following address & port::");
            Console.WriteLine("Address: {0}:{1}", ((IPEndPoint)listenSocket.LocalEndpoint).Address,
                ((IPEndPoint)listenSocket.LocalEndpoint).Port);
            Console.WriteLine("\n---@ Peer Activity @---");

            // Listen for new Peer connections
            while (true)
            {
                if (listenSocket.Pending())
                {
                    TcpClient newClient = listenSocket.AcceptTcpClient();
                    P2PRequest request = new P2PRequest(newClient);
                    request.Start();
                }
            }
        }

        public static void SendRequest(string fileName, string ip, int port)
        {
            Thread handler = new Thread(() => Get(fileName, ip, 8080)) { Priority = ThreadPriority.AboveNormal };
            handler.Start();
        }

        private static void Get(string fileName, string ip, int port)
        {
            IPAddress address = IPAddress.Parse(ip);
            IPEndPoint ep = new IPEndPoint(address, port);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(ep);
            client.Send(Encoding.UTF8.GetBytes("GET/" + fileName));
            while (client.Connected)
            {
                using (var stream = new NetworkStream(client))
                using (var output = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\Shared\\" + fileName))
                {
                    Console.WriteLine("Client connected. Starting to receive the file");

                    // read the file in chunks of 1KB
                    var buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                    output.Close();
                }
            }
        }
    }
}
