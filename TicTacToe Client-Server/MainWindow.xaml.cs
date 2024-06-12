using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToe_Client_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            App.BoardChanged += App_BoardChanged;
            App.PlayerSymbolMoveChanged += App_PlayerSymbolMoveChanged;
        }

        private void App_PlayerSymbolMoveChanged(object? sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.MoveSymbolLabel.Content = App.PlayerSymbolMove;

                Button[,] buttons = new Button[3, 3]
                {
                    { this.FindName("Button1") as Button, this.FindName("Button2") as Button, this.FindName("Button3") as Button },
                    { this.FindName("Button4") as Button, this.FindName("Button5") as Button, this.FindName("Button6") as Button },
                    { this.FindName("Button7") as Button, this.FindName("Button8") as Button, this.FindName("Button9") as Button }
                };

                bool enableButtons = (App.PlayerSymbolMove == App.PlayerSymbol);

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        buttons[i, j].IsEnabled = enableButtons;
                    }
                }
            });
        }

        private void App_BoardChanged(object? sender, EventArgs e)
        {
            UpdateButtonsFromBoard(App.Board);
        }

        private void OpenPanelButton_Click(object sender, RoutedEventArgs e)
        {
           TcpWIndow tcpWIndow = new TcpWIndow();
           tcpWIndow.Show();
        }

        public void UpdateButtonsFromBoard(char[,] board)
        {
            if (board == null) return;

            this.Dispatcher.Invoke(() =>
                    {
                        // Inicjalizacja dwuwymiarowej tablicy przycisków
                        Button[,] buttons = new Button[3, 3]
                        {
                           { this.FindName("Button1") as Button, this.FindName("Button2") as Button, this.FindName("Button3") as Button },
                           { this.FindName("Button4") as Button, this.FindName("Button5") as Button, this.FindName("Button6") as Button },
                           { this.FindName("Button7") as Button, this.FindName("Button8") as Button, this.FindName("Button9") as Button }
                        };
                        // Iteracja przez tablicę 3x3
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                char boardValue = board[i, j];

                                // Bezpieczna aktualizacja przycisku w wątku interfejsu użytkownika

                                if (i == 2 && j == 2)
                                    Console.Write(boardValue);

                                if (buttons[j, i] != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine(boardValue + " ->>>" + buttons[j, i].Name);
                                    buttons[j, i].Content = boardValue.ToString();
                                }

                            }
                        }
                    });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!App.TcpClient.IsConnected)
                return;

            if (App.PlayerSymbolMove != App.PlayerSymbol) 
                return;

            Debug.WriteLine($"move {App.PlayerSymbol}    {App.PlayerSymbolMove}");

            // Rzutujemy sender do typu Button, aby móc uzyskać do niego dostęp
            var button = sender as Button;
            if (button != null)
            {
                // Uzyskanie indeksów Grid.Row i Grid.Column
                int rowIndex = Grid.GetRow(button);
                int columnIndex = Grid.GetColumn(button);

                // Przeliczenie na zakres 1-3
                int x = columnIndex + 1;
                int y = rowIndex + 1;

                App.TcpClient.SendObject(new TcpPacketMove() { x = x, y = y, PlayerSymbol = App.PlayerSymbol });
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}