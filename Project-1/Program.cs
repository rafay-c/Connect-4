using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Conect4
{
    internal sealed class Computer
    {
        readonly int maximumDepth;
        readonly Random random;

        public Computer(DifficultyLevel difficultyLevel)
        {
            this.maximumDepth = (int)difficultyLevel;

            if (maximumDepth < (int)DifficultyLevel.Easy ||
                maximumDepth > (int)DifficultyLevel.Hard)
                throw new ArgumentOutOfRangeException("difficultyLevel");

            this.random = new Random(DateTime.Now.Millisecond);
        }

        public int GetBestMove(Board board, ActivePlayer player)
        {
            if (board == null)
                throw new ArgumentNullException("board");

            var node = new Node(board);
            var possibleMoves = getPossibleMoves(node);
            var scores = new double[possibleMoves.Count];
            Board updatedBoard;

            for (int i = 0; i < possibleMoves.Count; i++)
            {
                board.MakePlay(player, possibleMoves[i], out updatedBoard);
                var variant = new Node(updatedBoard);
                createTree(getOpponent(player), variant, 0);
                scores[i] = scoreNode(variant, player, 0);
            }

            double maximumScore = double.MinValue;
            var goodMoves = new List<int>();

            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] > maximumScore)
                {
                    goodMoves.Clear();
                    goodMoves.Add(i);
                    maximumScore = scores[i];
                }
                else if (scores[i] == maximumScore)
                {
                    goodMoves.Add(i);
                }
            }

            return possibleMoves[goodMoves[random.Next(0, goodMoves.Count)]];
        }

        private List<int> getPossibleMoves(Node node)
        {
            var moves = new List<int>();

            for (int i = 0; i < Board.BoardColumns; i++)
            {
                if (node.Board.GetCellState(0, i) == CellStates.Empty)
                {
                    moves.Add(i);
                }
            }

            return moves;
        }

        private void createTree(ActivePlayer player, Node rootNode, int depth)
        {
            if (depth >= maximumDepth)
                return;

            var moves = getPossibleMoves(rootNode);

            foreach (var move in moves)
            {
                Board updatedBoard;
                rootNode.Board.MakePlay(player, move, out updatedBoard);
                var variantNode = new Node(updatedBoard);
                createTree(getOpponent(player), variantNode, depth + 1);
                rootNode.Variants.Add(variantNode);
            }
        }

        private double scoreNode(Node node1, ActivePlayer player, int depth)
        {
            double score = 0;

            if (Ref.CheckForVictory(player, node1.Board))
            {
                if (depth == 0)
                {
                    score = double.PositiveInfinity;
                }
                else
                {
                    score += Math.Pow(10.0, maximumDepth - depth);
                }
            }
            else if (Ref.CheckForVictory(getOpponent(player), node1.Board))
            {
                score += -Math.Pow(100
                    , maximumDepth - depth);
            }
            else
            {
                foreach (var vContrincante in node1.Variants)
                {
                    score += scoreNode(vContrincante, player, depth + 1);
                }
            }

            return score;
        }

        private static ActivePlayer getOpponent(ActivePlayer opponent)
        {
            return opponent == ActivePlayer.Red ? ActivePlayer.Yellow : ActivePlayer.Red;
        }

        private class Node
        {
            readonly Board board;
            readonly List<Node> variants;

            public Board Board { get { return board; } }
            public List<Node> Variants { get { return variants; } }

            public Node(Board tablero)
            {
                this.board = tablero;
                this.variants = new List<Node>();
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var game = Game.CreateConsoleGame(DifficultyLevel.Hard, ActivePlayer.Yellow, false);
            game.Play();
        }
    }

    internal static class Ref
    {
        public const int requiredCellsInARow = 4;

        public static bool CheckForVictory(ActivePlayer player, Board tablero)
        {
            if (tablero == null)
                throw new ArgumentNullException("board");

            for (int i = 0; i < Board.BoardRows; i++)
            {
                for (int j = 0; j < Board.BoardColumns; j++)
                {
                    if (tablero.GetCellState(i, j) == (CellStates)player)
                    {
                        if (checkForVictory(tablero, i, j))
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool checkForVictory(Board board, int fil, int column)
        {
            bool searchRight, searchLeft, searchUp, searchDown;

            searchRight = column <= Board.BoardColumns - requiredCellsInARow;
            searchLeft = column >= requiredCellsInARow - 1;
            searchUp = fil > Board.BoardRows - requiredCellsInARow;
            searchDown = fil <= Board.BoardRows - requiredCellsInARow;

            if (searchRight)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil, column + 1),
                                    board.GetCellState(fil, column + 2),
                                    board.GetCellState(fil, column + 3)))
                    return true;
            }

            if (searchLeft)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil, column - 1),
                                    board.GetCellState(fil, column - 2),
                                    board.GetCellState(fil, column - 3)))
                    return true;
            }

            if (searchUp)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil - 1, column),
                                    board.GetCellState(fil - 2, column),
                                    board.GetCellState(fil - 3, column)))
                    return true;
            }

            if (searchDown)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil + 1, column),
                                    board.GetCellState(fil + 2, column),
                                    board.GetCellState(fil + 3, column)))
                    return true;
            }

            if (searchLeft && searchUp)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil - 1, column - 1),
                                    board.GetCellState(fil - 2, column - 2),
                                    board.GetCellState(fil - 3, column - 3)))
                    return true;
            }

            if (searchLeft && searchDown)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil + 1, column - 1),
                                    board.GetCellState(fil + 2, column - 2),
                                    board.GetCellState(fil + 3, column - 3)))
                    return true;
            }

            if (searchRight && searchUp)
            {
                if (checkCells(board.GetCellState(fil, column),
                                    board.GetCellState(fil - 1, column + 1),
                                    board.GetCellState(fil - 2, column + 2),
                                    board.GetCellState(fil - 3, column + 3)))
                    return true;
            }

            if (searchRight && searchDown)
            {
                if (checkCells(board.GetCellState(fil, column),
                               board.GetCellState(fil + 1, column + 1),
                                    board.GetCellState(fil + 2, column + 2),
                                    board.GetCellState(fil + 3, column + 3)))

                    return true;
            }

            return false;
        }

        private static bool checkCells(params CellStates[] celdas)
        {
            Debug.Assert(celdas.Length == requiredCellsInARow);

            for (int i = 1; i < requiredCellsInARow; i++)
            {
                if (celdas[i] != celdas[0])
                    return false;
            }

            return true;
        }
    }

    public class Game
    {
        private Board board;
        private Player humanPlayer, computerPlayer, activePlayer;
        private IODevice iODevice;

        public static Game CreateConsoleGame(DifficultyLevel difficultyLevel, ActivePlayer colorOrdenador, bool computerHasFirstTurn)
        {
            return new ConsoleGame(difficultyLevel, colorOrdenador, computerHasFirstTurn, IODevice.CreateConsoleDevice());
        }

        private Game(DifficultyLevel difficultyLevel, ActivePlayer computerColor, bool computerHasFirstTurn, IODevice iODevice)
        {
            board = Board.EmptyBoard;
            this.iODevice = iODevice;
        }

        public ActivePlayer ActivePlayerColor { get { return this.activePlayer.Color; } }
        public Board Board { get { return board; } }
        public IODevice UserInterface { get { return iODevice; } }

        private void changeActivePlayer()
        {
            if (activePlayer == humanPlayer)
            {
                activePlayer = computerPlayer;
            }
            else
            {
                activePlayer = humanPlayer;
            }
        }

        public virtual void Play()
        {
            activePlayer.RequestMove(board);
        }

        private class ConsoleGame : Game
        {
            public ConsoleGame(DifficultyLevel difficultyLevel, ActivePlayer computerColor, bool computerPlaysFirst, IODevice iODevice)
                : base(difficultyLevel, computerColor, computerPlaysFirst, iODevice)
            {
                computerPlayer = Player.CreateComputerPlayer(computerColor, difficultyLevel, iODevice);
                humanPlayer = Player.CreateHumanPlayer(computerColor == ActivePlayer.Red ? ActivePlayer.Yellow : ActivePlayer.Red, iODevice);

                if (computerPlaysFirst)
                {
                    activePlayer = computerPlayer;
                }
                else
                {
                    activePlayer = humanPlayer;
                }

                this.iODevice = iODevice;
            }

            public override void Play()
            {
                while (true)
                {
                    iODevice.Output("");
                    iODevice.Output(board.ToString());
                    iODevice.Output("");

                    int move = activePlayer.RequestMove(board);

                    if (!board.MakePlay(activePlayer.Color, move, out board))
                    {
                        iODevice.Output("Row is full. Try again.");
                        continue;
                    }

                    if (Ref.CheckForVictory(activePlayer.Color, board))
                    {
                        iODevice.Output(board.ToString());
                        iODevice.Output("");

                        if (activePlayer == computerPlayer)
                        {
                            iODevice.Accept("I'm sorry player {0}. I won again...", humanPlayer.Color);
                        }
                        else
                        {
                            iODevice.Accept("Congratulations player {0}! ¡You won!", humanPlayer.Color);
                        }
                        break;
                    }

                    if (board.NumberOfEmptyCells == 0)
                    {
                        iODevice.Output(board.ToString());
                        iODevice.Output("");
                        iODevice.Accept("¡Draw! I didnt loose...again");
                        break;
                    }

                    changeActivePlayer();
                }
            }
        }
    }

    internal class Player
    {
        public static Player CreateHumanPlayer(ActivePlayer color, IODevice iODevice)
        {
            return new HumanConsolePlayer(color, iODevice);
        }

        public static Player CreateComputerPlayer(ActivePlayer color, DifficultyLevel difficultyLevel, IODevice iODevice)
        {
            return new ComputerConsolePlayer(color, difficultyLevel, iODevice);
        }

        private readonly ActivePlayer playerColor;
        private readonly IODevice iODevice;

        private Player(ActivePlayer colorJugador, IODevice iODevice)
        {
            if (colorJugador != ActivePlayer.Red && colorJugador != ActivePlayer.Yellow)
                throw new ArgumentOutOfRangeException("playerColor");

            this.playerColor = colorJugador;
            this.iODevice = iODevice;
        }

        public ActivePlayer Color { get { return playerColor; } }

        public virtual int RequestMove(Board tablero)
        {
            return -1;
        }

        private class ComputerConsolePlayer : Player
        {
            private readonly Computer engine;

            public ComputerConsolePlayer(ActivePlayer color, DifficultyLevel difficulty, IODevice iODevice)
                : base(color, iODevice)
            {
                engine = new Computer(difficulty);
            }

            public override int RequestMove(Board board)
            {
                Debug.Assert(board != null);

                var move = engine.GetBestMove(board, playerColor);
                iODevice.Output("Player {0}'s turn. Hmmm...I'll play: {1}", playerColor, move);
                iODevice.Output("");
                return move;
            }
        }

        private class HumanConsolePlayer : Player
        {
            public HumanConsolePlayer(ActivePlayer color, IODevice iOdevice)
                : base(color, iOdevice) { }

            public override int RequestMove(Board tablero)
            {
                Debug.Assert(tablero != null);

                while (true)
                {
                    var input = (string)iODevice.Request("Player {0}'s turn: ", playerColor);
                    iODevice.Output("");
                    int jugada = -1;

                    if (int.TryParse(input, out jugada))
                    {
                        if (jugada < 0 || jugada >= Board.BoardColumns)
                        {
                            iODevice.Output("Column number must be within 0 and 6. Try again.");
                            continue;
                        }
                        else
                        {
                            return jugada;
                        }
                    }
                    else
                    {
                        iODevice.Output("'{0}' is not a column number. Try again.", input);
                    }
                }
            }
        }
    }

    public sealed class Board
    {
        public const int BoardRows = 6, BoardColumns = 7;
        public static readonly Board EmptyBoard = new Board();

        private readonly CellStates[,] cells;
        private readonly int numberOfEmptyCells;

        private Board()
        {
            cells = new CellStates[BoardRows, BoardColumns];
            numberOfEmptyCells = BoardRows * BoardColumns;
        }

        private Board(Board board, int numberOfEmptyCells)
        {
            if (board == null)
                throw new ArgumentNullException("board");

            if (numberOfEmptyCells < 0 || numberOfEmptyCells > BoardRows * BoardColumns)
                throw new ArgumentOutOfRangeException("numberOfEmptyCells");

            cells = new CellStates[BoardRows, BoardColumns];

            if (board != null)
            {
                for (int i = 0; i < BoardRows; i++)
                {
                    for (int j = 0; j < BoardColumns; j++)
                    {
                        cells[i, j] = board.cells[i, j];
                    }
                }
            }

            this.numberOfEmptyCells = numberOfEmptyCells;
        }

        public int NumberOfEmptyCells
        {
            get
            {
                return numberOfEmptyCells;
            }
        }

        public CellStates GetCellState(int row, int column)
        {
            if (row < 0 || row >= BoardRows)
                throw new ArgumentOutOfRangeException("row");

            if (column < 0 || column >= BoardColumns) throw new ArgumentOutOfRangeException("column");

            return cells[row, column];
        }

        public bool MakePlay(ActivePlayer player, int column, out Board board)
        {
            if (column < 0 || column >= BoardColumns) throw new ArgumentOutOfRangeException("column");

            if (cells[0, column] != CellStates.Empty)
            {
                board = this;
                return false;
            }

            board = new Board(this, numberOfEmptyCells - 1);

            int i;

            for (i = BoardRows - 1; i > -1; i--)
            {
                if (cells[i, column] == CellStates.Empty)
                    break;
            }

            board.cells[i, column] = (CellStates)player;
            return true;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            var header = "  0   1   2   3   4   5   6";
            var divisor = "-----------------------------";
            builder.AppendLine(header);
            builder.AppendLine(divisor);

            for (int i = 0; i < cells.GetLength(0); i++)
            {
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    var str = cells[i, j] == CellStates.Empty ? "| · " : (cells[i, j] == CellStates.Red ? "| X " : "| O ");
                    builder.Append(str);
                }

                builder.Append('|');
                builder.AppendLine();
                builder.AppendLine(divisor);
            }

            return builder.ToString(0, builder.Length - 1);
        }
    }

    public class IODevice
    {
        public static IODevice CreateConsoleDevice() { return new ConsoleDevice(); }
        private IODevice()
        {
        }

        public virtual void Output(string message, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public virtual object Request(string message, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public virtual void Accept(string message, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        private class ConsoleDevice : IODevice
        {
            public override void Output(string message, params object[] arguments)
            {
                Console.WriteLine(string.Format(message, arguments));
            }

            public override object Request(string message, params object[] arguments)
            {
                Console.Write(string.Format(message, arguments));
                return Console.ReadLine();
            }

            public override void Accept(string message, params object[] arguments)
            {
                Console.Write(string.Format(message, arguments));
                Console.ReadKey();
            }
        }
    }

    public enum ActivePlayer
    {
        Yellow = 1,
        Red = 2
    }

    public enum CellStates
    {
        Empty = 0,
        Yellow = ActivePlayer.Yellow,
        Red = ActivePlayer.Red
    }

    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 3,
        Hard = 4
    }
}