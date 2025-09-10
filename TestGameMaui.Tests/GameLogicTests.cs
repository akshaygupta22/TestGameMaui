using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestGameMaui.Core;

namespace TestGameMaui.Tests
{
    [TestClass]
    public class GameLogicTests
    {
        private GameLogic gameLogic;

        [TestInitialize]
        public void Setup()
        {
            gameLogic = new GameLogic();
        }

        [TestMethod]
        public void Constructor_InitializesWithDefaultValues()
        {
            Assert.AreEqual(3, gameLogic.Rows);
            Assert.AreEqual(3, gameLogic.Columns);
            Assert.AreEqual(5, gameLogic.Health);
            Assert.AreEqual(0, gameLogic.Score);
        }

        [TestMethod]
        public void GenerateMatrix_FillsWithValidNumbers()
        {
            for (int i = 0; i < gameLogic.Rows; i++)
            {
                for (int j = 0; j < gameLogic.Columns; j++)
                {
                    int value = gameLogic.GetValue(i, j);
                    Assert.IsTrue(value >= 1 && value <= 5, $"Value at [{i},{j}] is {value}, should be between 1 and 5");
                }
            }
        }

        [TestMethod]
        public void EvaluateSelection_CorrectSum_ReturnsTrue()
        {
            // Find two cells that sum to the target number
            var cells = new List<(int row, int col)>();
            int targetNumber = gameLogic.TargetNumber;
            bool foundMatch = false;

            for (int i = 0; i < gameLogic.Rows && !foundMatch; i++)
            {
                for (int j = 0; j < gameLogic.Columns && !foundMatch; j++)
                {
                    for (int k = 0; k < gameLogic.Rows && !foundMatch; k++)
                    {
                        for (int l = 0; l < gameLogic.Columns && !foundMatch; l++)
                        {
                            if (i == k && j == l) continue;
                            if (gameLogic.GetValue(i, j) + gameLogic.GetValue(k, l) == targetNumber)
                            {
                                cells.Add((i, j));
                                cells.Add((k, l));
                                foundMatch = true;
                            }
                        }
                    }
                }
            }

            if (foundMatch)
            {
                int initialScore = gameLogic.Score;
                bool result = gameLogic.EvaluateSelection(cells);
                
                Assert.IsTrue(result);
                Assert.AreEqual(initialScore + 10, gameLogic.Score);
                Assert.AreEqual(0, gameLogic.GetValue(cells[0].row, cells[0].col));
                Assert.AreEqual(0, gameLogic.GetValue(cells[1].row, cells[1].col));
            }
            else
            {
                Assert.Inconclusive("Could not find matching cells for test");
            }
        }

        [TestMethod]
        public void EvaluateSelection_WrongSum_ReturnsFalse()
        {
            // Find two cells that do NOT sum to the target number
            var cells = new List<(int row, int col)>();
            int targetNumber = gameLogic.TargetNumber;
            bool foundNonMatch = false;

            for (int i = 0; i < gameLogic.Rows && !foundNonMatch; i++)
            {
                for (int j = 0; j < gameLogic.Columns && !foundNonMatch; j++)
                {
                    for (int k = 0; k < gameLogic.Rows && !foundNonMatch; k++)
                    {
                        for (int l = 0; l < gameLogic.Columns && !foundNonMatch; l++)
                        {
                            if (i == k && j == l) continue;
                            if (gameLogic.GetValue(i, j) + gameLogic.GetValue(k, l) != targetNumber)
                            {
                                cells.Add((i, j));
                                cells.Add((k, l));
                                foundNonMatch = true;
                            }
                        }
                    }
                }
            }

            if (foundNonMatch)
            {
                int initialHealth = gameLogic.Health;
                int initialScore = gameLogic.Score;
                bool result = gameLogic.EvaluateSelection(cells);
                
                Assert.IsFalse(result);
                Assert.AreEqual(initialScore, gameLogic.Score);
                Assert.AreEqual(initialHealth - 1, gameLogic.Health);
            }
            else
            {
                Assert.Inconclusive("Could not find non-matching cells for test");
            }
        }

        [TestMethod]
        public void Reset_RestoresInitialState()
        {
            // Make some moves to change the game state
            var cells = new List<(int row, int col)> { (0, 0), (0, 1) };
            gameLogic.EvaluateSelection(cells);

            // Reset the game
            gameLogic.Reset();

            // Verify initial state is restored
            Assert.AreEqual(5, gameLogic.Health);
            Assert.AreEqual(0, gameLogic.Score);

            // Verify matrix has valid numbers
            for (int i = 0; i < gameLogic.Rows; i++)
            {
                for (int j = 0; j < gameLogic.Columns; j++)
                {
                    int value = gameLogic.GetValue(i, j);
                    Assert.IsTrue(value >= 1 && value <= 5);
                }
            }
        }

        [TestMethod]
        public void RefillMatrix_FillsZeroValues()
        {
            // Create some zeros in the matrix by making a correct match
            var cells = new List<(int row, int col)>();
            int targetNumber = gameLogic.TargetNumber;
            bool foundMatch = false;

            for (int i = 0; i < gameLogic.Rows && !foundMatch; i++)
            {
                for (int j = 0; j < gameLogic.Columns && !foundMatch; j++)
                {
                    for (int k = 0; k < gameLogic.Rows && !foundMatch; k++)
                    {
                        for (int l = 0; l < gameLogic.Columns && !foundMatch; l++)
                        {
                            if (i == k && j == l) continue;
                            if (gameLogic.GetValue(i, j) + gameLogic.GetValue(k, l) == targetNumber)
                            {
                                cells.Add((i, j));
                                cells.Add((k, l));
                                foundMatch = true;
                            }
                        }
                    }
                }
            }

            if (foundMatch)
            {
                gameLogic.EvaluateSelection(cells);
                gameLogic.RefillMatrix();

                // Verify no zeros remain
                for (int i = 0; i < gameLogic.Rows; i++)
                {
                    for (int j = 0; j < gameLogic.Columns; j++)
                    {
                        Assert.AreNotEqual(0, gameLogic.GetValue(i, j));
                    }
                }
            }
            else
            {
                Assert.Inconclusive("Could not find matching cells for test");
            }
        }

        [TestMethod]
        public void Health_ReachesZero_GameOver()
        {
            // Deliberately make wrong moves
            int initialHealth = gameLogic.Health;
            var wrongCells = new List<(int row, int col)> { (0, 0), (1, 1) };
            // Ensure this is a wrong move
            if (gameLogic.GetValue(0, 0) + gameLogic.GetValue(1, 1) == gameLogic.TargetNumber)
            {
                gameLogic.GenerateMatrix();
                gameLogic.GenerateTargetNumber();
            }
            for (int i = 0; i < initialHealth; i++)
            {
                gameLogic.EvaluateSelection(wrongCells);
            }
            Assert.AreEqual(0, gameLogic.Health);
        }

        [TestMethod]
        public void Score_AccumulatesOnMultipleCorrectAnswers()
        {
            int totalScore = 0;
            for (int round = 0; round < 3; round++)
            {
                // Find two cells that sum to the target number
                var cells = new List<(int row, int col)>();
                int targetNumber = gameLogic.TargetNumber;
                bool foundMatch = false;
                for (int i = 0; i < gameLogic.Rows && !foundMatch; i++)
                {
                    for (int j = 0; j < gameLogic.Columns && !foundMatch; j++)
                    {
                        for (int k = 0; k < gameLogic.Rows && !foundMatch; k++)
                        {
                            for (int l = 0; l < gameLogic.Columns && !foundMatch; l++)
                            {
                                if (i == k && j == l) continue;
                                if (gameLogic.GetValue(i, j) + gameLogic.GetValue(k, l) == targetNumber)
                                {
                                    cells.Add((i, j));
                                    cells.Add((k, l));
                                    foundMatch = true;
                                }
                            }
                        }
                    }
                }
                if (foundMatch)
                {
                    bool result = gameLogic.EvaluateSelection(cells);
                    Assert.IsTrue(result);
                    totalScore += 10;
                    Assert.AreEqual(totalScore, gameLogic.Score);
                    gameLogic.RefillMatrix();
                    gameLogic.GenerateTargetNumber();
                }
                else
                {
                    Assert.Inconclusive("Could not find matching cells for test");
                }
            }
        }

        [TestMethod]
        public void TargetNumber_IsAlwaysSumOfTwoCells()
        {
            for (int round = 0; round < 10; round++)
            {
                gameLogic.GenerateMatrix();
                gameLogic.GenerateTargetNumber();
                int found = 0;
                for (int i = 0; i < gameLogic.Rows; i++)
                {
                    for (int j = 0; j < gameLogic.Columns; j++)
                    {
                        for (int k = 0; k < gameLogic.Rows; k++)
                        {
                            for (int l = 0; l < gameLogic.Columns; l++)
                            {
                                if (i == k && j == l) continue;
                                if (gameLogic.GetValue(i, j) + gameLogic.GetValue(k, l) == gameLogic.TargetNumber)
                                {
                                    found++;
                                }
                            }
                        }
                    }
                }
                Assert.IsTrue(found > 0, $"No two cells sum to the target number {gameLogic.TargetNumber}");
            }
        }

        [TestMethod]
        public void Matrix_Values_AreAlwaysInRange()
        {
            for (int round = 0; round < 10; round++)
            {
                gameLogic.GenerateMatrix();
                for (int i = 0; i < gameLogic.Rows; i++)
                {
                    for (int j = 0; j < gameLogic.Columns; j++)
                    {
                        int value = gameLogic.GetValue(i, j);
                        Assert.IsTrue(value >= 1 && value <= 5, $"Value at [{i},{j}] is {value}, should be between 1 and 5");
                    }
                }
                gameLogic.RefillMatrix();
            }
        }
    }
}