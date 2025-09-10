using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TestGameMaui.Core;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TestGameMaui
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly GameLogic gameLogic;
        private ObservableCollection<ObservableCollection<CellViewModel>> matrix;
        private ObservableCollection<(int, int)> selectedCells = new();
        private int score;
        private int health;
        private int targetNumber;
        private bool isGameOver;
        private bool isEvaluating;
        public ICommand CellSelectedCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand ExitCommand { get; }

        // Event to notify the View to run an animation (raised when a correct match is made)
        public event EventHandler? StarAnimationRequested;
        // Event to notify the View that the game is over
        public event EventHandler? GameOverRequested;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel()
        {
            gameLogic = new GameLogic();
            score = gameLogic.Score;
            health = gameLogic.Health;
            targetNumber = gameLogic.TargetNumber;
            matrix = new ObservableCollection<ObservableCollection<CellViewModel>>();
            for (int i = 0; i < gameLogic.Rows; i++)
            {
                var row = new ObservableCollection<CellViewModel>();
                for (int j = 0; j < gameLogic.Columns; j++)
                {
                    row.Add(new CellViewModel(i, j, gameLogic.GetValue(i, j), CellSelected));
                }
                matrix.Add(row);
            }
            // wire async command wrapper
            CellSelectedCommand = new Command<CellViewModel>(async c => await CellSelected(c));
            NewGameCommand = new Command(NewGame);
            ExitCommand = new Command(Exit);
        }

        public ObservableCollection<ObservableCollection<CellViewModel>> Matrix
        {
            get => matrix;
            set { matrix = value; OnPropertyChanged(); }
        }
        public int Score { get => score; set { score = value; OnPropertyChanged(); } }
        public int Health { get => health; set { health = value; OnPropertyChanged(); } }
        public int TargetNumber { get => targetNumber; set { targetNumber = value; OnPropertyChanged(); } }
        public ObservableCollection<(int, int)> SelectedCells { get => selectedCells; set { selectedCells = value; OnPropertyChanged(); } }
        public bool IsGameOver { get => isGameOver; set { isGameOver = value; OnPropertyChanged(); } }
        public bool IsEvaluating { get => isEvaluating; set { isEvaluating = value; OnPropertyChanged(); UpdateCellEnabledState(); } }

        private async Task CellSelected(CellViewModel cell)
        {
            if (IsGameOver || IsEvaluating)
                return; // ignore selections when game over or while evaluating

            if (SelectedCells.Contains((cell.Row, cell.Col)))
            {
                SelectedCells.Remove((cell.Row, cell.Col));
                cell.IsSelected = false;
                cell.State = SelectionState.None;
            }
            else if (SelectedCells.Count < 2)
            {
                SelectedCells.Add((cell.Row, cell.Col));
                cell.IsSelected = true;
                cell.State = SelectionState.Selected;
            }
            if (SelectedCells.Count == 2)
            {
                await EvaluateSelection();
            }
        }

        private async Task EvaluateSelection()
        {
            // block input
            IsEvaluating = true;
            try
            {
                var selectedList = SelectedCells.ToList();
                bool correct = gameLogic.EvaluateSelection(selectedList);
                Score = gameLogic.Score;
                Health = gameLogic.Health;
                if (correct)
                {
                    // mark correct state so UI can show green
                    foreach (var (row, col) in selectedList)
                    {
                        Matrix[row][col].State = SelectionState.Correct;
                    }

                    // let UI show the green state briefly
                    await Task.Delay(400);

                    foreach (var (row, col) in selectedList)
                    {
                        Matrix[row][col].Value = 0;
                        Matrix[row][col].IsEnabled = false;
                        Matrix[row][col].IsSelected = false;
                        Matrix[row][col].State = SelectionState.None;
                    }

                    SelectedCells.Clear();
                    gameLogic.RefillMatrix();
                    UpdateMatrixFromLogic();
                    GenerateAndDisplayTargetNumber();

                    // Notify the view to play the star animation
                    StarAnimationRequested?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // mark wrong state so UI can show red
                    foreach (var (row, col) in selectedList)
                    {
                        Matrix[row][col].State = SelectionState.Wrong;
                    }

                    // let UI show the red state briefly
                    await Task.Delay(400);

                    foreach (var (row, col) in selectedList)
                    {
                        Matrix[row][col].IsSelected = false;
                        Matrix[row][col].State = SelectionState.None;
                    }
                    SelectedCells.Clear();

                    // If health dropped to zero or below, handle game over
                    if (gameLogic.Health <= 0)
                    {
                        IsGameOver = true;

                        // Disable all cells
                        for (int i = 0; i < Matrix.Count; i++)
                        {
                            for (int j = 0; j < Matrix[i].Count; j++)
                            {
                                Matrix[i][j].IsEnabled = false;
                            }
                        }

                        // Notify the view that game is over
                        GameOverRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                // unblock input
                IsEvaluating = false;
            }
        }

        private void UpdateMatrixFromLogic()
        {
            for (int i = 0; i < gameLogic.Rows; i++)
            {
                for (int j = 0; j < gameLogic.Columns; j++)
                {
                    Matrix[i][j].Value = gameLogic.GetValue(i, j);
                    Matrix[i][j].IsEnabled = gameLogic.GetValue(i, j) != 0 && !IsGameOver && !IsEvaluating;
                }
            }
        }

        private void UpdateCellEnabledState()
        {
            for (int i = 0; i < Matrix.Count; i++)
            {
                for (int j = 0; j < Matrix[i].Count; j++)
                {
                    // keep cells disabled if they are zero (removed), game over, or currently evaluating
                    bool enabled = Matrix[i][j].Value != 0 && !IsGameOver && !IsEvaluating;
                    Matrix[i][j].IsEnabled = enabled;
                }
            }
        }

        public void GenerateAndDisplayTargetNumber()
        {
            gameLogic.GenerateTargetNumber();
            TargetNumber = gameLogic.TargetNumber;
        }

        private void NewGame()
        {
            gameLogic.Reset();
            Score = gameLogic.Score;
            Health = gameLogic.Health;
            IsGameOver = false;
            IsEvaluating = false;
            UpdateMatrixFromLogic();
            GenerateAndDisplayTargetNumber();
        }

        private void Exit()
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif WINDOWS
            Application.Current.Quit();
#elif IOS
            // iOS apps are not supposed to exit programmatically
#else
            Application.Current.Quit();
#endif
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SelectionState { None, Selected, Correct, Wrong }

    public class CellViewModel : INotifyPropertyChanged
    {
        public int Row { get; }
        public int Col { get; }
        private int value;
        private bool isSelected;
        private bool isEnabled = true;
        private SelectionState state = SelectionState.None;
        public ICommand SelectCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public CellViewModel(int row, int col, int value, Func<CellViewModel, Task> onSelect)
        {
            Row = row;
            Col = col;
            this.value = value;
            SelectCommand = new Command(async () => await onSelect(this));
        }
        public int Value { get => value; set { this.value = value; OnPropertyChanged(); } }
        public bool IsSelected { get => isSelected; set { isSelected = value; OnPropertyChanged(); } }
        public bool IsEnabled { get => isEnabled; set { isEnabled = value; OnPropertyChanged(); } }
        public SelectionState State { get => state; set { state = value; OnPropertyChanged(); } }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
