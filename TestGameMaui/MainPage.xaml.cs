using TestGameMaui.Core;
using System.ComponentModel;

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
        }

        private async void ViewModel_GameOverRequested(object? sender, EventArgs e)
        {
            // Ensure running on UI thread
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await ShowGameOverPopup());
                return;
            }

            await ShowGameOverPopup();
        }

        private async Task ShowGameOverPopup()
        {
            // Show popup with Restart and Exit options
            string title = "Game Over";
            string message = $"Your score: {viewModel.Score}.\nDo you want to restart or exit?";
            bool restart = await DisplayAlert(title, message, "Restart", "Exit");
            if (restart)
            {
                // Restart the game via command
                if (viewModel.NewGameCommand != null && viewModel.NewGameCommand.CanExecute(null))
                    viewModel.NewGameCommand.Execute(null);
            }
            else
            {
                // Exit the app via command
                if (viewModel.ExitCommand != null && viewModel.ExitCommand.CanExecute(null))
                    viewModel.ExitCommand.Execute(null);
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
            double areaWidth = 400;
            double areaHeight = 400;
            double starSize = 32;
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
