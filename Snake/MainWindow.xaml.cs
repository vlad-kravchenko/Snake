using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Snake
{
    public partial class MainWindow : Window
    {
        enum Direction { LEFT, RIGHT, UP, DOWN }
        Random rand = new Random();
        DispatcherTimer timer;
        Direction direction;
        List<Point> path = new List<Point>();
        bool stopped = false;
        int points;

        public MainWindow()
        {
            InitializeComponent();
            SetupGame();
        }

        private void SetupGame()
        {
            points = 0;
            InfoBar.Text = $"Score 25 points. Points scored: 0";
            SetupGrid();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 520);
            timer.Start();
            direction = Direction.RIGHT;
            path.Clear();
            stopped = false;
        }

        private void SetupGrid()
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.Children.Clear();
            for (int i = 0; i < 20; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            MainGrid.Children.Add(Head);
            Grid.SetColumn(Head, 5);
            Grid.SetRow(Head, 10);
        }

        private void timerTick(object sender, EventArgs e)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    if (!CanMove(Direction.LEFT)) return;
                    TryMoveSnake(Grid.GetRow(Head), Grid.GetColumn(Head) - 1);
                    break;
                case Direction.RIGHT:
                    if (!CanMove(Direction.RIGHT)) return;
                    TryMoveSnake(Grid.GetRow(Head), Grid.GetColumn(Head) + 1);
                    break;
                case Direction.UP:
                    if (!CanMove(Direction.UP)) return;
                    TryMoveSnake(Grid.GetRow(Head) - 1, Grid.GetColumn(Head));
                    break;
                case Direction.DOWN:
                    if (!CanMove(Direction.DOWN)) return;
                    TryMoveSnake(Grid.GetRow(Head) + 1, Grid.GetColumn(Head));
                    break;
            }
            if (stopped) return;

            RemoveOldBody();
            DisplayBody();

            if (GetFoodOnMap().Count() == 0)
                AddFood();
        }

        private List<UIElement> GetFoodOnMap()
        {
            return MainGrid.Children.Cast<UIElement>().Where(t => (t as Image).Tag != null && (t as Image).Tag.ToString() == "Food").ToList();
        }

        private void DisplayBody()
        {
            path.Reverse();
            foreach (var pathPoint in path.Take(points))
            {
                var newPart = new Image
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Stretch = System.Windows.Media.Stretch.Fill,
                    Source = Resources["Body"] as BitmapImage,
                    Tag = "Body"
                };
                MainGrid.Children.Add(newPart);
                Grid.SetRow(newPart, (int)pathPoint.X);
                Grid.SetColumn(newPart, (int)pathPoint.Y);
            }
            path.Reverse();
        }

        private void RemoveOldBody()
        {
            var oldBody = MainGrid.Children.Cast<UIElement>().Where(t => (t as Image).Tag != null && (t as Image).Tag.ToString() == "Body").ToList();
            for (int i = 0; i < oldBody.Count() - 1; i++)
            {
                MainGrid.Children.Remove(oldBody[i]);
            }
        }

        private void AddFood()
        {
            int row = rand.Next(0, 20);
            int col = rand.Next(0, 20);
            if (CanMove(row, col))
            {
                var food = new Image
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Stretch = System.Windows.Media.Stretch.Fill,
                    Source = Resources["Food"] as BitmapImage,
                    Tag = "Food"
                };
                MainGrid.Children.Add(food);
                Grid.SetRow(food, row);
                Grid.SetColumn(food, col);
            }
        }

        private void TryMoveSnake(int row, int col)
        {
            if ((row < 0 || row > MainGrid.RowDefinitions.Count - 1) ||
                (col < 0 || col > MainGrid.ColumnDefinitions.Count - 1) || 
                (row == Grid.GetRow(Head) && col == Grid.GetColumn(Head)))
            {
                stopped = true;
                return;
            }
            stopped = false;
            Grid.SetRow(Head, row);
            Grid.SetColumn(Head, col);
            path.Add(new Point(row, col));
        }

        private bool CanMove(Direction dir)
        {
            int row = Grid.GetRow(Head);
            int col = Grid.GetColumn(Head);
            switch (dir)
            {
                case Direction.LEFT:
                    if (col == 0) return false;
                    return CanMove(row, col - 1);
                case Direction.RIGHT:
                    if (col > MainGrid.ColumnDefinitions.Count - 1) return false;
                    return CanMove(row, col + 1);
                case Direction.UP:
                    if (row == 0) return false;
                    return CanMove(row - 1, col);
                case Direction.DOWN:
                    if (col > MainGrid.RowDefinitions.Count - 1) return false;
                    return CanMove(row + 1, col);
            }
            return false;
        }

        private bool CanMove(int row, int col)
        {
            var children = MainGrid.Children
                          .Cast<UIElement>()
                          .Where(i => Grid.GetRow(i) == row && Grid.GetColumn(i) == col).ToList();
            if (ContainsFood(children))
            {
                MainGrid.Children.Remove(children.First());
                points += 1;
                timer.Interval = new TimeSpan(0, 0, 0, 0, timer.Interval.Milliseconds - 20);
                if (timer.Interval.Milliseconds == 20)
                {
                    timer.Stop();
                    InfoBar.Text = $"Victory! Game over! Press Enter to restart.";
                }
                else
                {
                    InfoBar.Text = $"Score 25 points. Points scored: " + points;
                }
                return true;
            }
            return children.Count() == 0;
        }

        private bool ContainsFood(List<UIElement> children)
        {
            return children.Count() != 0 && (children.First() as Image).Tag.ToString() == "Food";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (direction == Direction.RIGHT) return;
                    direction = Direction.LEFT;
                    break;
                case Key.Right:
                    if (direction == Direction.LEFT) return;
                    direction = Direction.RIGHT;
                    break;
                case Key.Up:
                    if (direction == Direction.DOWN) return;
                    direction = Direction.UP;
                    break;
                case Key.Down:
                    if (direction == Direction.UP) return;
                    direction = Direction.DOWN;
                    break;
                case Key.Space:
                    timer.IsEnabled = !timer.IsEnabled;
                    break;
                case Key.Enter:
                    if (!timer.IsEnabled)
                        SetupGame();
                    break;
            }
        }
    }
}