using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public class ServerManager
    {
        public TcpClient MyClient { get; set; }
        public string Success { get; set; }
        public ServerManager(string ip, string port) 
        {
            MyClient = new TcpClient(ip, Convert.ToInt32(port));
            SendFiles();

            Byte[] bytes = new Byte[MyClient.Client.ReceiveBufferSize];
            int received = MyClient.Client.Receive(bytes);

            Success = Encoding.UTF8.GetString(bytes);
        }

        public void SendFiles() 
        {
            string[] dirs = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Shared\\");
            string files = "";
            for (int i = 0; i < dirs.Length; i++) 
            {
                files += Path.GetFileName(dirs[i].ToString()) + "/";
            }

            Byte[] send = Encoding.UTF8.GetBytes("UPDATE/" + dirs.Length + "/" + files);
            MyClient.Client.SendBufferSize = send.Length;
            MyClient.Client.Send(send);
        }

        public List<string> GetPeers(string fileName) 
        {
            Byte[] send = Encoding.UTF8.GetBytes("SEARCH/" + fileName);
            MyClient.Client.Send(send);

            Byte[] recieve = new Byte[MyClient.Client.ReceiveBufferSize];
            MyClient.Client.Receive(recieve);

            string[] files = Encoding.UTF8.GetString(recieve).TrimEnd('\0').Split('/');

            List<string> temp = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                temp.Add(files[i]);
            }

            return temp;
        }

        public void Close() 
        {
            Byte[] send = Encoding.UTF8.GetBytes("CLOSE");
            MyClient.Client.Send(send);
        }

        public List<string> GetList() 
        {
            Byte[] send = Encoding.UTF8.GetBytes("GET");
            MyClient.Client.Send(send);

            Byte[] recieve = new Byte[MyClient.Client.ReceiveBufferSize];
            MyClient.Client.Receive(recieve);

            string[] files = Encoding.UTF8.GetString(recieve).TrimEnd('\0').Split('/');

            List<string> temp = new List<string>();
            for (int i = 0; i < files.Length; i++) 
            {
                temp.Add(files[i]);
            }

            return temp;
        }

    }
}
