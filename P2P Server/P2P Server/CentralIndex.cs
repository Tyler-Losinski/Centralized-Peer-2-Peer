using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CentralIndex
    {
        private static int _counter;

        private Dictionary<int, Peer> PeerList { get; set; }
        public Dictionary<string, List<int>> FileList { get; set; }

        // Constructor
        public CentralIndex()
        {
            PeerList = new Dictionary<int,Peer>();
            FileList = new Dictionary<string,List<int>>();
        }

        // Adds a Peer to the index
        public int AddPeer(TcpClient clientSocket)
        {
            // Get hostname of client, then break up into IP and port
            string remoteIp = ((IPEndPoint) clientSocket.Client.RemoteEndPoint).Address.ToString();
            int remotePort = ((IPEndPoint) clientSocket.Client.RemoteEndPoint).Port;

            // Use LINQ to see if that user is already in the index
            int id = (from entry in PeerList let tempPeer = entry.Value where tempPeer.IpAddress.Equals(remoteIp)
                          && tempPeer.Port == remotePort select entry.Key).FirstOrDefault();

            // User is already in index; just return the existing id
            if (id > 0)
                return id;

            // Increment user counter, add the new Peer to the directory, and assign the Peer an id
            _counter++;
            Peer newPeer = new Peer { IpAddress = remoteIp, Port = remotePort };
            PeerList.Add(_counter, newPeer);
            return _counter;
        }

        // Updates the index to show that the Peer with the specified id has the specified file
        // If the index already shows the Peer has this file, this method does nothing
        public void AddFile(TcpClient clientSocket, String fileName, int id)
        {
            int chk = 0;
            List<int> currentList = new List<int>();

            // Use LINQ to see if the file exists in the index with other peers
            if (FileList.Select(entry => entry.Key).Any(file => file.ToUpper().Equals(fileName.ToUpper())))
                chk = 1;

            // If file already is in index, assert that the Peer is listed as having the file
            if (chk == 1)
            {
                FileList.TryGetValue(fileName, out currentList);
                if (currentList != null && !currentList.Contains(id))
                {
                    currentList.Add(id);
                    try
                    {
                        FileList.Add(fileName, currentList);
                    }
                    catch (Exception) { }
                }
            }

            // File is not in index; add it, and assert that the Peer has it
            else
            {
                currentList.Add(id);
                FileList.Add(fileName, currentList);
            }
        }

        // Returns a List of all Peer ids that have the given file
        public List<int> SearchFiles(String fileName)
        {
            List<int> theList;
            FileList.TryGetValue(fileName, out theList);
            return theList;
        }

        // Returns the Peer object for a given Peer id
        public Peer SearchPeers(int pId)
        {
            Peer thePeer;
            PeerList.TryGetValue(pId, out thePeer);
            return thePeer;
        }

        // Removes the Peer matching the given id from the index
        // This method will also remove the Peer's id from all file records
        public void RemovePeer(TcpClient clientSocket, int id)
        {
            PeerList.Remove(id);

            Dictionary<String, List<int>>.ValueCollection vals = FileList.Values;
            foreach (List<int> tempList in vals)
            {
                tempList.Remove(id);
            }
        }

        // Not entirely sure what this method is supposed to be for, to be honest.
        public List<int> ReplicateFiles(int num, int id)
        {
            List<int> currentList = new List<int>();

            if ((PeerList.Count - 1) >= num)
                currentList.AddRange(PeerList.Select(entry => entry.Key).Where(peerId => peerId != id));
            else
                currentList = null;

            return currentList;
        }

    }
}
