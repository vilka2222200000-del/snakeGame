using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace snakeGame
{
    public partial class MainWindow : Window
    {
        private const int CanvasSize = 600;

        private readonly Random random = new Random();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly IRecordStorage recordStorage;

        private HighScores highScores;
        private List<Cell> snake = new List<Cell>();
        private List<Cell> obstacles = new List<Cell>();
        private Cell food;
        private Direction direction = Direction.Right;
        private Direction nextDirection = Direction.Right;
        private GameMode gameMode = GameMode.Classic;
        private int rows = 20;
        private int columns = 20;
        private bool isGameStarted;
        private bool isPaused;

        public event EventHandler ScoreChanged;
        public event EventHandler GameOver;

        public MainWindow()
        {
            InitializeComponent();

            string recordsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "records.xml");
            recordStorage = new XmlRecordStorage(recordsPath);
            highScores = recordStorage.Load();

            timer.Tick += Timer_Tick;
            ScoreChanged += MainWindow_ScoreChanged;
            GameOver += MainWindow_GameOver;

            DrawGame();
            UpdateInfo();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isGameStarted)
                return;

            isPaused = !isPaused;
            StatusText.Text = isPaused ? "Пауза." : "Игра идет.";
            Focus();
        }

        private void ModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gameMode = ModeBox.SelectedIndex == 1 ? GameMode.Hard : GameMode.Classic;
            UpdateInfo();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartGame();
                return;
            }

            if (e.Key == Key.Space)
            {
                PauseButton_Click(sender, e);
                return;
            }

            if (!isGameStarted || isPaused)
                return;

            if (e.Key == Key.Up && direction != Direction.Down)
                nextDirection = Direction.Up;
            else if (e.Key == Key.Down && direction != Direction.Up)
                nextDirection = Direction.Down;
            else if (e.Key == Key.Left && direction != Direction.Right)
                nextDirection = Direction.Left;
            else if (e.Key == Key.Right && direction != Direction.Left)
                nextDirection = Direction.Right;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void StartGame()
        {
            columns = ReadSize(ColumnsBox.Text, 20);
            rows = ReadSize(RowsBox.Text, 20);
            gameMode = ModeBox.SelectedIndex == 1 ? GameMode.Hard : GameMode.Classic;

            snake.Clear();
            obstacles.Clear();

            int startX = columns / 2;
            int startY = rows / 2;
            snake.Add(new Cell(startX, startY));
            snake.Add(new Cell(startX - 1, startY));
            snake.Add(new Cell(startX - 2, startY));

            direction = Direction.Right;
            nextDirection = Direction.Right;
            isGameStarted = true;
            isPaused = false;

            if (gameMode == GameMode.Hard)
                CreateObstacles();

            CreateFood();
            timer.Interval = TimeSpan.FromMilliseconds(GetSpeed());
            timer.Start();

            StatusText.Text = "Игра идет.";
            DrawGame();
            UpdateInfo();
            Focus();
        }

        private void MoveSnake()
        {
            if (!isGameStarted || isPaused)
                return;

            direction = nextDirection;

            Cell head = snake[0];
            Cell newHead = GetNextCell(head);
            bool eatFood = newHead == food;

            if (IsWall(newHead) || IsObstacle(newHead) || IsSnakeCollision(newHead, eatFood))
            {
                FinishGame();
                return;
            }

            snake.Insert(0, newHead);

            if (eatFood)
            {
                CreateFood();
                timer.Interval = TimeSpan.FromMilliseconds(GetSpeed());
                OnScoreChanged();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            DrawGame();
            UpdateInfo();
        }

        private Cell GetNextCell(Cell head)
        {
            if (direction == Direction.Up)
                return new Cell(head.X, head.Y - 1);

            if (direction == Direction.Down)
                return new Cell(head.X, head.Y + 1);

            if (direction == Direction.Left)
                return new Cell(head.X - 1, head.Y);

            return new Cell(head.X + 1, head.Y);
        }

        private void CreateFood()
        {
            List<Cell> freeCells = new List<Cell>();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Cell cell = new Cell(x, y);
                    if (!snake.Contains(cell) && !obstacles.Contains(cell))
                        freeCells.Add(cell);
                }
            }

            if (freeCells.Count == 0)
            {
                FinishGame();
                return;
            }

            food = freeCells[random.Next(freeCells.Count)];
        }

        private void CreateObstacles()
        {
            int count = Math.Min(35, rows * columns / 18);

            while (obstacles.Count < count)
            {
                Cell cell = new Cell(random.Next(columns), random.Next(rows));

                if (!snake.Contains(cell) && !obstacles.Contains(cell))
                    obstacles.Add(cell);
            }
        }

        private void FinishGame()
        {
            timer.Stop();
            isGameStarted = false;
            StatusText.Text = "Игра окончена. Длина змейки: " + snake.Count + ".";
            OnGameOver();
            DrawGame();
            UpdateInfo();
        }

        private void MainWindow_ScoreChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void MainWindow_GameOver(object sender, EventArgs e)
        {
            if (highScores.SaveIfBest(gameMode, snake.Count))
                recordStorage.Save(highScores);
        }

        private void OnScoreChanged()
        {
            EventHandler handler = ScoreChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnGameOver()
        {
            EventHandler handler = GameOver;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private bool IsWall(Cell cell)
        {
            return cell.X < 0 || cell.X >= columns || cell.Y < 0 || cell.Y >= rows;
        }

        private bool IsObstacle(Cell cell)
        {
            return obstacles.Contains(cell);
        }

        private bool IsSnakeCollision(Cell cell, bool eatFood)
        {
            if (!snake.Contains(cell))
                return false;

            Cell tail = snake[snake.Count - 1];
            return eatFood || cell != tail;
        }

        private int GetSpeed()
        {
            int speed = 180 - (snake.Count - 3) * 6;

            if (gameMode == GameMode.Hard)
                speed -= 20;

            return Math.Max(55, speed);
        }

        private int ReadSize(string text, int defaultValue)
        {
            int value;
            if (!int.TryParse(text, out value))
                value = defaultValue;

            if (value < 10)
                value = 10;

            if (value > 40)
                value = 40;

            return value;
        }

        private void UpdateInfo()
        {
            if (ScoreText == null || highScores == null)
                return;

            ScoreText.Text = snake.Count.ToString();
            BestScoreText.Text = highScores.GetBest(gameMode).ToString();
        }

        private void DrawGame()
        {
            if (GameCanvas == null)
                return;

            GameCanvas.Children.Clear();

            double cellWidth = (double)CanvasSize / columns;
            double cellHeight = (double)CanvasSize / rows;

            DrawGrid(cellWidth, cellHeight);

            foreach (Cell obstacle in obstacles)
                DrawRectangle(obstacle, cellWidth, cellHeight, Brushes.DimGray);

            DrawEllipse(food, cellWidth, cellHeight, Brushes.Firebrick);

            for (int i = snake.Count - 1; i >= 0; i--)
            {
                Brush brush = i == 0 ? Brushes.ForestGreen : Brushes.LimeGreen;
                DrawRectangle(snake[i], cellWidth, cellHeight, brush);
            }
        }

        private void DrawGrid(double cellWidth, double cellHeight)
        {
            for (int x = 0; x <= columns; x++)
            {
                Line line = new Line();
                line.X1 = x * cellWidth;
                line.Y1 = 0;
                line.X2 = x * cellWidth;
                line.Y2 = CanvasSize;
                line.Stroke = Brushes.LightGray;
                GameCanvas.Children.Add(line);
            }

            for (int y = 0; y <= rows; y++)
            {
                Line line = new Line();
                line.X1 = 0;
                line.Y1 = y * cellHeight;
                line.X2 = CanvasSize;
                line.Y2 = y * cellHeight;
                line.Stroke = Brushes.LightGray;
                GameCanvas.Children.Add(line);
            }
        }

        private void DrawRectangle(Cell cell, double cellWidth, double cellHeight, Brush brush)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = cellWidth - 1;
            rectangle.Height = cellHeight - 1;
            rectangle.Fill = brush;
            Canvas.SetLeft(rectangle, cell.X * cellWidth);
            Canvas.SetTop(rectangle, cell.Y * cellHeight);
            GameCanvas.Children.Add(rectangle);
        }

        private void DrawEllipse(Cell cell, double cellWidth, double cellHeight, Brush brush)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = cellWidth - 3;
            ellipse.Height = cellHeight - 3;
            ellipse.Fill = brush;
            Canvas.SetLeft(ellipse, cell.X * cellWidth + 1.5);
            Canvas.SetTop(ellipse, cell.Y * cellHeight + 1.5);
            GameCanvas.Children.Add(ellipse);
        }
    }
}
