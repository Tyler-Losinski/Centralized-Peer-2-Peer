using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * @author Adam Hart
 * 
 * Modified from Zeeshan Aamir Khavas's Java application:
 * "Centralized P2P Application"
 */

namespace P2P_Server
{
    public class Request
    {
        readonly TcpClient _clientSocket;
        readonly CentralIndex _index;
        readonly int _id;

        // Constructor
        public Request(TcpClient clientSocket, CentralIndex index, int id)
        {
            _clientSocket = clientSocket;
            _index = index;
            _id = id;
        }

        /*
         * Listen for requests from the client.  All reqeusts have a header code followed by
         * a forward slash '/', and possibly a payload. The following commands are recognized:
         * 
         *   UPDATE    - Update the client's list of available files.
         *               Payload should consist of a 32-bit signed integer indicating the number
         *                 of files the client has, immediately followed by a list of all
         *                 file names. Each file name should be followed by a forward slash '/'.
         *               The server does not give a response.
         * 
         *   SEARCH    - Client requests a list of hostnames for all connected peers that have a
         *                 specified file.
         *               Payload should consist only of the name of the desired file followed by
         *                 a forward slash '/'.
         *               The server will respond with a 32-bit signed integer of how many peers
         *                 have the file, immediately followed by a list of peer hostnames in the
         *                 format "X.X.X.X:Port/"
         *               
         *   REPLICATE - Honestly, I have no idea. Included just in case it might be useful.
         *   
         *   CLOSE     - Closes the connection.  No payload or reply from server.
         */
        public void Parser()
        {
            int flag = 0;

		    try
            {
			    while(_clientSocket.Connected)
                {
                    Byte[] buff = new Byte[_clientSocket.Client.ReceiveBufferSize];
                    _clientSocket.Client.Receive(buff);

                    string request = Encoding.UTF8.GetString(buff).ToUpper().TrimEnd('\0');
                    string[] requests = request.Split('/');

                    // Detected "UDPATE" command
                    if (requests[0].Equals("UPDATE"))
                    {
                        // Read in the number of files the client has
                        int files = Convert.ToInt32(requests[1]);

                        for (int i = 2; i < files + 2; i++)
                        {
                            // Read a single file name
                            string fileName = String.Empty;
                            fileName = requests[i].ToString();
                            _index.AddFile(_clientSocket, fileName, _id);

                            Console.WriteLine(fileName);
                        }


                    }

                    // Detected "SEARCH" command
                    else if (requests[0].Equals("SEARCH"))
                    {
                        // Read in the name of the requested file
                        string fileName = requests[1].ToString();

                        // Get the list of peers that have the requested file
                        List<int> currentList = _index.SearchFiles(fileName);
                        int size = currentList.Count;

                        string files = "";
                        foreach (int peer in currentList)
                        {
                            Peer resPeer = _index.SearchPeers(peer);
                            files += resPeer.FullHost() + "/";
                            
                        }
                        Byte[] bytes = Encoding.UTF8.GetBytes(files);
                        _clientSocket.Client.Send(bytes);

                    }

                    // Detected "REPLICATE" command
                    else if (requests[0].Equals("GET"))
                    {
                        List<string> stuff = new List<string>(_index.FileList.Keys);
                        string list = "";
                        foreach (string s in stuff) 
                        {
                            list += s + "/";
                        }

                        Byte[] bytes = Encoding.UTF8.GetBytes(list);
                        _clientSocket.Client.Send(bytes);

                    }

                    // Detected "CLOSE" command
                    else if (requests[0].Equals("CLOSE"))
                    {
                        flag = 1;
                        _index.RemovePeer(_clientSocket, _id);
                        _clientSocket.Close();
                        Console.WriteLine("Peer [ {0}:{1} ] disconnected", ((IPEndPoint)_clientSocket.Client.RemoteEndPoint).Address,
                            ((IPEndPoint)_clientSocket.Client.RemoteEndPoint).Port);

                    }
			    }
		    }
		    catch (IOException)
            {
			    if(flag != 1)
                {
                    Console.WriteLine("Peer [ {0}:{1} ] disconnected", ((IPEndPoint)_clientSocket.Client.RemoteEndPoint).Address,
                        ((IPEndPoint)_clientSocket.Client.RemoteEndPoint).Port);
			    }
		    }
	    }
    }
}
