using System.Configuration;
using System.Data;
using System.Net.Sockets;
using System.Windows;

namespace TicTacToe_Client_Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TcpServer TcpServer { get; set; }
        public static TcpSender TcpClient { get; set; }

        public static char PlayerSymbol { get; set; }

        public static char[,] Board
        {
            get
            {
                return _board;
            }
            set
            {
                _board = value;
                BoardOnValueChanged(EventArgs.Empty);
            }
        }

        private static char[,] _board;
        public static event EventHandler BoardChanged;

        protected static void BoardOnValueChanged(EventArgs e)
        {
            BoardChanged?.Invoke(null, e);
        }

        public static char PlayerSymbolMove
        {
            get
            {
                return _playerSymbolMove;
            }
            set
            {
                _playerSymbolMove = value;
                PlayerSymbolMoveOnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// which player can currently move
        /// </summary>
        private static char _playerSymbolMove;
        public static event EventHandler PlayerSymbolMoveChanged;

        protected static void PlayerSymbolMoveOnValueChanged(EventArgs e)
        {
            PlayerSymbolMoveChanged?.Invoke(null, e);
        }
    }
}
