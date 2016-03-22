using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Net.Sockets;
using System.Net;

namespace P2PClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ServerManager serverManager;
        public MainWindow()
        {
            InitializeComponent();

            LogIn form = new LogIn();      
            form.ShowDialog();
            serverManager = form.ServerConnection;

            FilesL.ItemsSource = serverManager.GetList();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FilesL.Items.GetItemAt(FilesL.Items.IndexOf(FilesL.SelectedItem)).ToString();
            List<string> temp = new List<string>();
            temp = serverManager.GetPeers(fileName);
            string[] ipPort = temp[0].Split(':');

            if (!ipPort[0].Equals(GetLocalIPAddress()))
            {
                P2PManager.SendRequest(fileName, ipPort[0], Convert.ToInt32(ipPort[1]));
                MessageBox.Show("File Downloaded!");
            }
            else
                MessageBox.Show("File already exists on your computer");

           
        }
        private void RefreshB_Click(object sender, RoutedEventArgs e)
        {
            FilesL.ItemsSource = serverManager.GetList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            serverManager.Close();
            Environment.Exit(0);
        }


    }
}
