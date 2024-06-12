using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TicTacToe_Client_Server.Packets;
using Console = System.Diagnostics.Debug;

namespace TicTacToe_Client_Server
{
    public class TcpSender
    {
        private TcpClient tcpClient;

        public bool IsConnected => tcpClient.Connected;

        private string server;
        private int port;

        public void Connect(string server, int port)
        {
            this.server = server;
            this.port = port;
            this.tcpClient = new TcpClient();
            this.tcpClient.Connect(server, port);
            Console.WriteLine("Połączono z serwerem.");

            Thread clientThread = new Thread(() => HandleClientComm(tcpClient)) { IsBackground = true };
            clientThread.Start();
        }

        public void SendObject<T>(T obj)
        {
            NetworkStream stream = this.tcpClient.GetStream();
            string json = PacketHelper.Serialize(obj);
            byte[] dataToSend = Encoding.UTF8.GetBytes(json + Environment.NewLine);
            stream.Write(dataToSend, 0, dataToSend.Length);
            Console.WriteLine("Wysłano obiekt: " + json);
        }
        private void HandleClientComm(TcpClient tcpClient)
        {
            NetworkStream clientStream = tcpClient.GetStream();
            StreamReader reader = new StreamReader(clientStream);

            while (true)
            {
                // Odczytaj całą wiadomość jako string JSON
                string jsonMessage = reader.ReadLine();
                if (jsonMessage == null) break; // Jeśli brak danych, zakończ pętlę

                string[] splittedJsonMessage = jsonMessage.Split('|');
                string messageType = splittedJsonMessage[0];
                string messageContent = splittedJsonMessage[1];

                switch (messageType)
                {
                    case "TcpPacketConnectionSucceed":
                        var connection = JsonConvert.DeserializeObject<TcpPacketConnectionSucceed>(messageContent);
                        Console.WriteLine(connection.PlayerSymbol);
                        App.PlayerSymbol = connection.PlayerSymbol;
                        break;
                    case "TcpPacketBoard":
                        var packetBoard = JsonConvert.DeserializeObject<TcpPacketBoard>(messageContent);
                        App.Board = packetBoard.board;
                        break;
                    case "TcpPacketWin":
                        var winState = JsonConvert.DeserializeObject<TcpPacketWin>(messageContent);
                        App.Board = new char[3, 3];
                        if (MessageBox.Show($"Gre wygrał gracz z symbolem: {winState.PlayerSymbol}", "Wynik gry", MessageBoxButton.OK) == MessageBoxResult.OK)
                        {
                            //Connect(server, port);
                        }
                        break;
                    case "TcpPacketMove":
                        var move = JsonConvert.DeserializeObject<TcpPacketMove>(messageContent);
                        App.PlayerSymbolMove = move.PlayerSymbol;
                        
                        Debug.WriteLine($"receive move {App.PlayerSymbol}    {App.PlayerSymbolMove}");
                        break;
                }
            }
        }
    }
}
