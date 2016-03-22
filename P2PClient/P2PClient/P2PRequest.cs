using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PClient
{
    class P2PRequest
    {

        private readonly TcpClient _clientSocket;

        // Constructor
        public P2PRequest(TcpClient clientSocket)
        {
            _clientSocket = clientSocket;
        }

        // Starts a seperate thread for the client
        public void Start()
        {
            //Console.WriteLine("Peer [ {0} ] connected", ((IPEndPoint) _clientSocket.Client.RemoteEndPoint).Address);
            Thread handler = new Thread(ProcessClient) {Priority = ThreadPriority.AboveNormal};
            handler.Start();
        }

        // Handles listening for the client
        private void ProcessClient()
        {
            try
            {
                Byte[] bytes = new byte[_clientSocket.ReceiveBufferSize];
                int received = _clientSocket.Client.Receive(bytes);

                string[] response = Encoding.UTF8.GetString(bytes).Split('/');

                if (response[0].ToUpper().TrimEnd('\0').Equals("GET"))
                {
                    string fileName = response[1].TrimEnd('\0');
                    Byte[] fileBytes = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "//Shared//" + fileName);
                    _clientSocket.Client.Send(fileBytes);
                }
		    }
		    catch (IOException)
            {
                //Console.WriteLine("Peer [ {0}:{1} ] disconnected", ((IPEndPoint) _clientSocket.Client.RemoteEndPoint).Address,
                //    ((IPEndPoint) _clientSocket.Client.RemoteEndPoint).Port);
		    }
        }

    }
}
