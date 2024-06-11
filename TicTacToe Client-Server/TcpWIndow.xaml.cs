using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace TicTacToe_Client_Server
{
    /// <summary>
    /// Logika interakcji dla klasy TcpWIndow.xaml
    /// </summary>
    public partial class TcpWIndow : Window
    {
        public TcpWIndow()
        {
            InitializeComponent();
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            const int _port = 1111;

            App.TcpServer = new TcpServer(_port);
            Thread serverThread = new Thread(() => App.TcpServer.Start());
            serverThread.Start();
            Console.WriteLine($"Serwer został uruchomiony na wątku {serverThread.ManagedThreadId} i nasłuchuje na porcie {_port}.");

            //this.IpTextBoxHost.Text = IPAddress.Any.Address.ToString();
            this.PortTextBoxHost.Text = _port.ToString();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            App.TcpClient = new TcpSender();
            App.TcpClient.Connect(this.IpTextBoxClient.Text, int.Parse(this.PortTextBoxClient.Text));
        }

        private void TestConnectButton_Click(object sender, RoutedEventArgs e)
        {
            App.TcpClient.SendObject(new TcpPacketBoard()
            {
                board = new char[,] {
                        {'X', 'O', 'X'},
                        {'O', 'X', 'O'},
                        {'X', 'O', 'X'}
                }
            });
        }
    }
}
