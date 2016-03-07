using System;
using System.Collections.Generic;
using System.IO;
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
        public void Parser(NetworkStream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            int flag = 0;

		    try
            {
			    while(_clientSocket.Connected)
                {		
                    // Read in the command
				    String code = String.Empty;
                    char next = reader.ReadChar();
                    while (!next.Equals('/'))
                    {
                        code += next;
                        next = reader.ReadChar();
                    }
                    code = code.ToUpper();

                    // Detected "UDPATE" command
				    if (code.Equals("UPDATE"))
                    {
                        // Read in the number of files the client has
                        int files = reader.ReadInt32();

					    for (int i = 0; i < files; i++)
                        {
                            // Read a single file name
						    string fileName = String.Empty;
                            next = reader.ReadChar();
                            while (!next.Equals('/'))
                            {
                                fileName += next;
                                next = reader.ReadChar();
                            }
						    _index.AddFile(_clientSocket, fileName, _id);
					    }

				    }

				    // Detected "SEARCH" command
				    else if (code.Equals("SEARCH"))
                    {
                        // Read in the name of the requested file
                        string fileName = String.Empty;
                        next = reader.ReadChar();
                        while (!next.Equals('/'))
                        {
                            fileName += next;
                            next = reader.ReadChar();
                        }

                        // Get the list of peers that have the requested file
					    List<int> currentList = _index.SearchFiles(fileName);
					    int size = currentList.Count;

                        // If the client already has the file, discount them
					    if (currentList.Remove(_id)) --size;

                        // Return the number of peers
					    writer.Write(size);

                        // If there are more than zero peers, return each one
                        if (size == 0) continue;
                        foreach (int peer in currentList)
                            SendPeers(peer, writer);
				    }

				    // Detected "REPLICATE" command
                    else if (code.Equals("REPLICATE"))
                    {
                        int num = reader.ReadInt32();
                        reader.ReadByte();

					    List<int> currentList = _index.ReplicateFiles(num, _id);

					    if (currentList != null)
                        {
						    int size = currentList.Count;
						    writer.Write(size);
                            writer.Write('/');
						    foreach (int peer in currentList)
                                SendPeers(peer, writer);
					    }
					    else{
						    writer.Write(0+"/");
					    }

				    }

				    // Detected "CLOSE" command
				    else if(code.Equals("CLOSE")){
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

        // Used to write a Peer to the stream
        private void SendPeers(int pId, StreamWriter writer)
        {
		    Peer resPeer = _index.SearchPeers(pId);
            writer.Write(resPeer.FullHost() + "/");
	    }
    }
}
