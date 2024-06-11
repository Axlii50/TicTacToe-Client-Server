using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using TicTacToe_Client_Server.Packets;
using System.Diagnostics;

namespace TicTacToe_Client_Server
{
    public class TcpServer
    {
        private TcpListener tcpListener;
        private int port;

        public GameData gameData;

        private List<(TcpClient Tcpclient, char Symbol)> clients;
        private Dictionary<TcpClient, char> clientsDict;

        public TcpServer(int port)
        {
            this.port = port;
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            //this.clients = new();
            this.clientsDict = new();
            this.gameData = new GameData();
        }

        public void Start()
        {
            this.tcpListener.Start();
            System.Diagnostics.Debug.WriteLine("Serwer uruchomiony na porcie " + port + ".");
            ListenForClients();
        }

        private void ListenForClients()
        {
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Console.WriteLine("Połączenie przychodzące...");
                var symbol = UnUsedSymbol();

                lock (clientsDict)
                {
                    clientsDict.Add(client, symbol);
                }

                //send assigned symbol to user
                BroadcastObjectToClient(new TcpPacketConnectionSucceed() { PlayerSymbol = symbol }, client);
                //send current moving player
                BroadcastObjectToClients(new TcpPacketMove() { PlayerSymbol = gameData.CurrentPlayerTurn });

                Thread clientThread = new Thread(() => HandleClientComm(client)) { IsBackground = true };
                clientThread.Start();
            }
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

                Debug.WriteLine(messageContent);

                switch (messageType)
                {
                    case "String":
                        System.Diagnostics.Debug.WriteLine(jsonMessage);
                        break;
                    case nameof(TcpPacketMove):
                        var packetMove = JsonConvert.DeserializeObject<TcpPacketMove>(messageContent);

                        if (this.gameData.CurrentPlayerTurn != packetMove.PlayerSymbol)
                        {
                            BroadcastObjectToClients(new TcpPacketBoard() { board = gameData.Board });
                            break;
                        }

                        //Update board
                        if (gameData.Board[packetMove.x - 1, packetMove.y - 1] == '\0')
                        {
                            gameData.Board[packetMove.x - 1, packetMove.y - 1] = packetMove.PlayerSymbol;
                            this.gameData.CurrentPlayerTurn = (this.gameData.CurrentPlayerTurn == 'X') ? 'O' : 'X';
                        }
                        //gameData.CurrentPlayerTurn = ReverseSymbol(gameData.CurrentPlayerTurn);
                        BroadcastObjectToClients(new TcpPacketBoard() { board = gameData.Board });
                        BroadcastObjectToClients(new TcpPacketMove() { PlayerSymbol = gameData.CurrentPlayerTurn });

                        Debug.WriteLine(gameData.CurrentPlayerTurn);

                        //check for win
                        var gameState = TicTacToeHelper.CheckWinner(gameData.Board);
                        if (gameState != '\0')
                        {
                            gameData.Board = new char[3, 3];
                            isXUsed = isOUsed = false;
                            BroadcastObjectToClients(new TcpPacketWin() { PlayerSymbol = gameState });
                            BroadcastObjectToClients(new TcpPacketBoard() { board = gameData.Board });

                            //send assigned symbol to user
                            foreach (var client in clientsDict)
                            {
                                var symbol = UnUsedSymbol();

                                clientsDict[client.Key] = symbol;

                                BroadcastObjectToClient(new TcpPacketConnectionSucceed() { PlayerSymbol = symbol }, client.Key);
                            }
                        }

                            break;
                    case nameof(TcpPacketBoard): // możliwe ze bedzie do wywalenia
                        var packetBoard = JsonConvert.DeserializeObject<TcpPacketBoard>(messageContent);

                        int rows = packetBoard.board.GetLength(0); // Pobiera liczbę wierszy
                        int columns = packetBoard.board.GetLength(1); // Pobiera liczbę kolumn

                        for (int i = 0; i < rows; i++) // Iteracja przez wiersze
                        {
                            for (int j = 0; j < columns; j++) // Iteracja przez kolumny
                            {
                                System.Diagnostics.Debug.Write(packetBoard.board[i, j] + " "); // Wyświetlanie pojedynczego elementu
                            }
                            System.Diagnostics.Debug.WriteLine(""); // Nowa linia po każdym wierszu
                        }

                        break;
                    case nameof(TcpPacketWin): //możliwe ze bedzie do wywalenia
                        var packetWin = JsonConvert.DeserializeObject<TcpPacketWin>(messageContent);
                        //System.Diagnostics.Debug.WriteLine($"Ruch gracza {packet.PlayerSymbol}: ({packet.x}, {packet.y})");
                        break;
                }
            }

            tcpClient.Close();
        }

        public void BroadcastObjectToClients<T>(T obj)
        {
            // Serializacja obiektu do JSON
            string json = PacketHelper.Serialize(obj);
            byte[] bytesToSend = Encoding.ASCII.GetBytes(json + Environment.NewLine); // Dodanie nowej linii na końcu ułatwia odczyt po stronie klienta

            lock (clientsDict)
            {
                for (int i = clientsDict.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        NetworkStream stream = clientsDict.ElementAt(i).Key.GetStream();
                        if (stream.CanWrite)
                        {
                            stream.Write(bytesToSend, 0, bytesToSend.Length);
                        }
                    }
                    catch (Exception)
                    {
                        // W przypadku błędu usuń klienta z listy
                        try
                        {
                            clientsDict.ElementAt(i).Key.Close();
                        }
                        catch { /* Ignorowanie błędów przy zamykaniu */ }
                        clientsDict.Remove(clientsDict.ElementAt(i).Key);
                    }
                }
            }
        }

        public void BroadcastObjectToClient<T>(T obj, TcpClient client)
        {
            string json = PacketHelper.Serialize(obj);
            byte[] bytesToSend = Encoding.ASCII.GetBytes(json + Environment.NewLine); // Dodanie nowej linii na końcu ułatwia odczyt po stronie klienta

            try
            {
                NetworkStream stream = client.GetStream();
                if (stream.CanWrite)
                {
                    stream.Write(bytesToSend, 0, bytesToSend.Length);
                }
            }
            catch (Exception)
            {
                //do something?
            }
        }

        bool isXUsed = false;
        bool isOUsed = false;
        public char UnUsedSymbol()
        {
            if (!isXUsed)
            {
                isXUsed = true;
                return 'X';
            }
            if (!isOUsed)
            {
                isOUsed = true;
                return 'O';
            }

            return ' ';
        }

        public char ReverseSymbol(char symbol)
        {
            if (symbol == 'X') return 'O';
            if (symbol == 'O') return 'X';
            return ' ';
        }
    }
}
