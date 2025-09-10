using TestGameMaui.Core;
using System.ComponentModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System.Reflection;

namespace TestGameMaui
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel viewModel;
        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainPageViewModel();
            BindingContext = viewModel;
            viewModel.StarAnimationRequested += ViewModel_StarAnimationRequested;
            viewModel.GameOverRequested += ViewModel_GameOverRequested;
            viewModel.SoundRequested += ViewModel_SoundRequested;

            this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, EventArgs e)
        {
            // Make the game area cover ~80% of the smaller dimension
            double width = this.Width;
            double height = this.Height;
            if (width <= 0 || height <= 0)
                return;

            double smaller = Math.Min(width, height);
            double gameAreaSize = smaller * 0.65; // 65% of smaller dimension

            // Set GameAreaGrid size
            GameAreaGrid.WidthRequest = gameAreaSize;
            GameAreaGrid.HeightRequest = gameAreaSize;

            // Determine cell size based on a 3x3 grid with padding (3 columns)
            int columns = 3; // current game logic uses 3
            double paddingPerCell = 4 * 2; // margins around frames
            double available = gameAreaSize - (columns * paddingPerCell);
            double cellSize = Math.Floor(available / columns);

            // Update dynamic resources
            this.Resources["CellSize"] = cellSize;
            this.Resources["CellFontSize"] = Math.Max(18, cellSize * 0.5);
            this.Resources["ScoreFontSize"] = Math.Max(20, smaller * 0.06);
            this.Resources["TargetFontSize"] = Math.Max(28, smaller * 0.09);
            this.Resources["HeartSize"] = Math.Max(20, smaller * 0.06);
        }

        private async void ViewModel_SoundRequested(object? sender, SoundType e)
        {
            if (!viewModel.IsSoundEnabled)
                return;

            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(() => HandleSound(e));
                return;
            }

            HandleSound(e);
        }

        private void HandleSound(SoundType type)
        {
            try
            {
                // Simple vibration feedback as haptics fallback
                try
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50));
                }
                catch { }

                // Additional sound playback can be implemented later
            }
            catch { }
        }

        private async void ViewModel_GameOverRequested(object? sender, EventArgs e)
        {
            // in-page overlay handles the UI, nothing to do here, but ensure we run on UI thread
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(() => { });
            }
        }

        private async void ViewModel_StarAnimationRequested(object? sender, EventArgs e)
        {
            // Ensure running on UI thread
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await ShowStarAnimation());
                return;
            }

            await ShowStarAnimation();
        }

        // Only keep animation logic here
        private async Task ShowStarAnimation()
        {
            double areaWidth = GameAreaGrid.WidthRequest > 0 ? GameAreaGrid.WidthRequest : 400;
            double areaHeight = GameAreaGrid.HeightRequest > 0 ? GameAreaGrid.HeightRequest : 400;
            double starSize = Math.Max(24, Math.Min(areaWidth, areaHeight) * 0.08);
            double halfStar = starSize / 2;
            var stars = new List<Image>();

            // 8 positions: corners and edge centers
            var positions = new (double x, double y)[]
            {
                // Corners
                (0, 0),
                (areaWidth - starSize, 0),
                (0, areaHeight - starSize),
                (areaWidth - starSize, areaHeight - starSize),
                // Edge centers
                (areaWidth / 2 - halfStar, 0),
                (areaWidth / 2 - halfStar, areaHeight - starSize),
                (0, areaHeight / 2 - halfStar),
                (areaWidth - starSize, areaHeight / 2 - halfStar)
            };

            foreach (var (x, y) in positions)
            {
                var star = new Image
                {
                    Source = "star.png",
                    Opacity = 0,
                    HeightRequest = starSize,
                    WidthRequest = starSize,
                    Scale = 0.5
                };
                AbsoluteLayout.SetLayoutFlags(star, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(star, new Rect(x, y, starSize, starSize));
                StarOverlay.Children.Add(star);
                stars.Add(star);
            }

            // Animate stars popping
            var tasks = new List<Task>();
            foreach (var star in stars)
            {
                tasks.Add(star.ScaleTo(1.5, 250, Easing.SpringOut));
                tasks.Add(star.FadeTo(1, 250));
            }
            await Task.WhenAll(tasks);
            await Task.Delay(500);

            // Fade out with rotation
            var fadeTasks = new List<Task>();
            foreach (var star in stars)
            {
                fadeTasks.Add(star.FadeTo(0, 250));
                fadeTasks.Add(star.RotateTo(360, 250));
            }
            await Task.WhenAll(fadeTasks);
            foreach (var star in stars)
            {
                StarOverlay.Children.Remove(star);
            }
        }
    }
}
