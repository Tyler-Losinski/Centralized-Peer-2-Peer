using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/*
 * @author Adam Hart
 * 
 * Modified from Zeeshan Aamir Khavas's Java application:
 * "Centralized P2P Application"
 */

namespace P2P_Server
{
    public class PeerManager
    {
        private readonly TcpClient _clientSocket;
        private readonly CentralIndex _index;
        private NetworkStream _stream;
        private Request _req;
        private StreamWriter _writer;

        // Constructor
        public PeerManager(TcpClient clientSocket, CentralIndex index)
        {
            _clientSocket = clientSocket;
            _index = index;
        }

        // Starts a seperate thread for the client
        public void Start()
        {
            Console.WriteLine("Peer [ {0} ] connected", ((IPEndPoint) _clientSocket.Client.RemoteEndPoint).Address);
            Thread handler = new Thread(ProcessClient) {Priority = ThreadPriority.AboveNormal};
            handler.Start();
        }

        // Handles listening for the client
        private void ProcessClient()
        {
            try
            {
                _stream = _clientSocket.GetStream();
                _writer = new StreamWriter(_stream);

                // Add the client to the index as a Peer
                int id = _index.AddPeer(_clientSocket);

                // Reply to client with "1" if registration was successful, "0" otherwise
                _writer.Write(id > 0 ? "1/" : "0/");

                // Handle further client requests in a Request object
                _req = new Request(_clientSocket, _index, id);
			    _req.Parser(_stream);
		    }
		    catch (IOException)
            {
                Console.WriteLine("Peer [ {0}:{1} ] disconnected", ((IPEndPoint) _clientSocket.Client.RemoteEndPoint).Address,
                    ((IPEndPoint) _clientSocket.Client.RemoteEndPoint).Port);
		    }
        }
    }
}
