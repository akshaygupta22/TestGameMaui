using Foundation;
using Microsoft.Maui;
using UIKit;

namespace TestGameMaui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            var result = base.FinishedLaunching(application, launchOptions);

#if MACCATALYST
            foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
            {
                if (scene is UIWindowScene windowScene && windowScene.Titlebar != null)
                {
                    windowScene.Titlebar.TitleVisibility = UITitlebarTitleVisibility.Hidden;
                    windowScene.Titlebar.Toolbar = null;
                }
            }
#endif
            return result;
        }
    }
}
