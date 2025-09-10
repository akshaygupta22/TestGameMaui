namespace TestGameMaui.Core
{
    public class GameLogic
    {
        private readonly int[,] matrix;
        private readonly int rows;
        private readonly int columns;
        private readonly Random random;
        private int targetNumber;
        private int health;
        private int score;

        public GameLogic(int rows = 3, int columns = 3)
        {
            this.rows = rows;
            this.columns = columns;
            matrix = new int[rows, columns];
            random = new Random();
            health = 5;
            score = 0;
            GenerateMatrix();
            GenerateTargetNumber();
        }

        public int Health => health;
        public int Score => score;
        public int TargetNumber => targetNumber;
        public int Rows => rows;
        public int Columns => columns;

        public int GetValue(int row, int col) => matrix[row, col];

        public void GenerateMatrix()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = random.Next(1, 6);
                }
            }
        }

        public void GenerateTargetNumber()
        {
            var cells = new List<(int, int)>();
            while (cells.Count < 2)
            {
                int r = random.Next(rows);
                int c = random.Next(columns);
                var cell = (r, c);
                if (!cells.Contains(cell))
                    cells.Add(cell);
            }
            targetNumber = 0;
            foreach (var (r, c) in cells)
                targetNumber += matrix[r, c];
        }

        public bool EvaluateSelection(List<(int row, int col)> selectedCells)
        {
            if (selectedCells.Count != 2)
                return false;

            int sum = selectedCells.Sum(cell => matrix[cell.row, cell.col]);
            
            if (sum == targetNumber)
            {
                foreach (var (row, col) in selectedCells)
                {
                    matrix[row, col] = 0;
                }
                score += 10;
                return true;
            }
            else
            {
                health--;
                return false;
            }
        }

        public void RefillMatrix()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (matrix[i, j] == 0)
                    {
                        matrix[i, j] = random.Next(1, 6);
                    }
                }
            }
        }

        public void Reset()
        {
            health = 5;
            score = 0;
            GenerateMatrix();
            GenerateTargetNumber();
        }
    }
}