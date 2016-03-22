using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace P2PClient
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        public ServerManager ServerConnection { get; set; }
        public LogIn()
        {
            InitializeComponent();
        }

        private void LogInB_Click(object sender, RoutedEventArgs e)
        {
            ServerManager conn = new ServerManager(IPTxt.Text, PortTxt.Text);
            P2PManager.StartListener(Convert.ToInt32(PortTxt.Text));
            ServerConnection = conn;
            this.Close();
        }
    }
}
