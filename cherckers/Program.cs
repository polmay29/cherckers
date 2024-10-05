using System;

class Program
{
    static char[,] board = new char[8, 8];
    static char currentPlayer;
    static (int, int) lastMoveStart = (-1, -1);
    static bool captureRequired = false;
    static bool multipleCapture = false;

    static void Main(string[] args)
    {
        Console.WriteLine("Выберите режим игры:");
        Console.WriteLine("1. Обычный режим");
        Console.WriteLine("2. Белые - 1 дамка, Черные - 5 пешек");
        int mode = int.Parse(Console.ReadLine());

        InitializeBoard(mode);
        DrawBoard();

        currentPlayer = 'w';
        while (true)
        {
            Console.WriteLine($"{(currentPlayer == 'w' ? "Белые" : "Черные")} ходят.");
            Console.WriteLine("Введите \"сдаться\", чтобы прекратить игру.");

            if (CheckCaptureAvailability(currentPlayer))
            {
                Console.WriteLine("У вас есть возможность рубить. Если вы не рубите, шашка сгорит.");
                captureRequired = true;
            }
            else
            {
                captureRequired = false;
            }

            Console.Write("Введите начальную позицию (x y) или \"сдаться\": ");
            string startInput = Console.ReadLine();
            if (startInput.ToLower() == "сдаться")
            {
                Console.WriteLine($"{(currentPlayer == 'w' ? "Белые" : "Черные")} сдались. Игра окончена.");
                break;
            }

            string[] startCoords = startInput.Split();
            int startX = int.Parse(startCoords[0]);
            int startY = int.Parse(startCoords[1]);

            Console.Write("Введите конечную позицию (x y): ");
            string[] endInput = Console.ReadLine().Split();
            int endX = int.Parse(endInput[0]);
            int endY = int.Parse(endInput[1]);

            if (IsValidMove(startX, startY, endX, endY))
            {
                MakeMove(startX, startY, endX, endY);
                DrawBoard();

                if (multipleCapture)
                {
                    while (CanCapture(endX, endY))
                    {
                        Console.WriteLine("Вы можете продолжить рубить. Сделайте следующий ход.");
                        Console.Write("Введите конечную позицию (x y) для следующего взятия: ");
                        endInput = Console.ReadLine().Split();
                        int nextX = int.Parse(endInput[0]);
                        int nextY = int.Parse(endInput[1]);

                        if (IsValidMove(endX, endY, nextX, nextY))
                        {
                            MakeMove(endX, endY, nextX, nextY);
                            DrawBoard();
                            endX = nextX;
                            endY = nextY;
                        }
                        else
                        {
                            Console.WriteLine("Неверный ход. Попробуйте снова.");
                        }
                    }
                    multipleCapture = false;
                }

                if (captureRequired && lastMoveStart != (startX, startY))
                {
                    Console.WriteLine("Вы не сделали обязательное взятие. Ваша шашка сгорает.");
                    board[startY, startX] = ' ';
                    captureRequired = false;
                }

                currentPlayer = currentPlayer == 'w' ? 'b' : 'w';
            }
            else
            {
                Console.WriteLine("Неверный ход. Попробуйте снова.");
            }
        }
    }

    static void InitializeBoard(int mode)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if ((x + y) % 2 == 1)
                {
                    if (mode == 1)
                    {
                        if (y < 3) board[y, x] = 'b';
                        else if (y > 4) board[y, x] = 'w';
                        else board[y, x] = ' ';
                    }
                    else if (mode == 2)
                    {
                        if (y == 7 && x == 0) board[y, x] = 'D';
                        else if (y < 3) board[y, x] = 'b';
                        else board[y, x] = ' ';
                    }
                }
                else
                {
                    board[y, x] = ' ';
                }
            }
        }
    }

    static void DrawBoard()
    {
        Console.Clear();
        Console.WriteLine("   0   1   2   3   4   5   6   7 ");
        Console.WriteLine("  +---+---+---+---+---+---+---+---+");
        for (int y = 0; y < 8; y++)
        {
            Console.Write($"{y} |");
            for (int x = 0; x < 8; x++)
            {
                Console.Write($" {board[y, x]} |");
            }
            Console.WriteLine();
            Console.WriteLine("  +---+---+---+---+---+---+---+---+");
        }
    }

    static bool IsValidMove(int startX, int startY, int endX, int endY)
    {
        if (!IsInsideBoard(startX, startY) || !IsInsideBoard(endX, endY)) return false;
        if (board[startY, startX] == ' ') return false;
        if (board[endY, endX] != ' ') return false;

        char piece = board[startY, startX];
        if ((currentPlayer == 'w' && (piece == 'b' || piece == 'B')) || (currentPlayer == 'b' && (piece == 'w' || piece == 'D')))
        {
            return false;
        }

        int deltaX = Math.Abs(endX - startX);
        int deltaY = Math.Abs(endY - startY);

        if (deltaX == 1 && deltaY == 1 && !captureRequired)
        {
            return true;
        }

        if (deltaX == 2 && deltaY == 2)
        {
            int midX = (startX + endX) / 2;
            int midY = (startY + endY) / 2;
            if (IsEnemyPiece(midX, midY, currentPlayer))
            {
                multipleCapture = true;
                return true;
            }
        }

        if (char.IsUpper(piece))
        {
            return IsValidKingMove(startX, startY, endX, endY, piece);
        }

        return false;
    }

    static bool IsValidKingMove(int startX, int startY, int endX, int endY, char piece)
    {
        int deltaX = Math.Abs(endX - startX);
        int deltaY = Math.Abs(endY - startY);

        if (deltaX != deltaY) return false;

        int stepX = (endX > startX) ? 1 : -1;
        int stepY = (endY > startY) ? 1 : -1;

        int x = startX + stepX;
        int y = startY + stepY;

        bool foundEnemy = false;

        while (x != endX && y != endY)
        {
            if (board[y, x] != ' ')
            {
                if (IsEnemyPiece(x, y, currentPlayer))
                {
                    if (foundEnemy) return false;
                    foundEnemy = true;
                }
                else
                {
                    return false;
                }
            }
            x += stepX;
            y += stepY;
        }

        return foundEnemy;
    }

    static bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    static bool IsEnemyPiece(int x, int y, char player)
    {
        char enemy = player == 'w' ? 'b' : 'w';
        return board[y, x] == enemy || board[y, x] == char.ToUpper(enemy);
    }

    static bool CheckCaptureAvailability(char player)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (board[y, x] == player || board[y, x] == char.ToUpper(player))
                {
                    if (CanCapture(x, y)) return true;
                }
            }
        }
        return false;
    }

    static bool CanCapture(int startX, int startY)
    {
        char piece = board[startY, startX];
        bool isKing = char.IsUpper(piece);

        int[] dx = { -2, -2, 2, 2 };
        int[] dy = { -2, 2, -2, 2 };

        for (int i = 0; i < 4; i++)
        {
            int newX = startX + dx[i];
            int newY = startY + dy[i];

            if (IsInsideBoard(newX, newY) &&
                IsEnemyPiece(startX + dx[i] / 2, startY + dy[i] / 2, currentPlayer) && board[newY, newX] == ' ')
            {
                return true;
            }
        }

        return false;
    }

    static void MakeMove(int startX, int startY, int endX, int endY)
    {
        char piece = board[startY, startX];

        int stepX = (endX > startX) ? 1 : -1;
        int stepY = (endY > startY) ? 1 : -1;


        int x = startX + stepX;
        int y = startY + stepY;

        bool captured = false;

        while (x != endX && y != endY)
        {
            if (board[y, x] != ' ')
            {
                if (IsEnemyPiece(x, y, currentPlayer))
                {

                    board[y, x] = ' ';
                    captured = true;
                }
                else
                {

                    return;
                }
            }
            x += stepX;
            y += stepY;
        }


        board[endY, endX] = piece;
        board[startY, startX] = ' ';


        if (endY == 0 && piece == 'w') board[endY, endX] = 'D';
        if (endY == 7 && piece == 'b') board[endY, endX] = 'B';

        lastMoveStart = (startX, startY);


        if (captured)
        {
            while (CanCapture(endX, endY))
            {
                Console.WriteLine("Вы можете продолжить рубить. Сделайте следующий ход.");
                int nextX, nextY;

                Console.Write("Введите конечную позицию (x y) для следующего взятия: ");
                string[] nextInput = Console.ReadLine().Split();
                nextX = int.Parse(nextInput[0]);
                nextY = int.Parse(nextInput[1]);


                if (IsValidMove(endX, endY, nextX, nextY))
                {

                    MakeMove(endX, endY, nextX, nextY);
                    DrawBoard();
                    endX = nextX;
                    endY = nextY;
                }
                else
                {
                    Console.WriteLine("Неверный ход. Попробуйте снова.");
                }
            }
        }
    }
}
