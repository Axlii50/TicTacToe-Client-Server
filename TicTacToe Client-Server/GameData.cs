namespace TicTacToe_Client_Server
{
    public class GameData
    {
        public char[,] Board { get; set; }

        public char CurrentPlayerTurn;

        public GameData()
        {
            Board = new char[3,3];
            CurrentPlayerTurn = new Random().Next(2) == 0 ? 'X' : 'O';
        }
    }
}