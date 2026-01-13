using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using KnolageTests.Pages;

namespace KnolageTests
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize
            | ConfigChanges.Orientation
            | ConfigChanges.UiMode
            | ConfigChanges.ScreenLayout
            | ConfigChanges.SmallestScreenSize
            | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            HandleIntent(Intent);
            RequestNotificationPermissionIfNeeded();
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private void HandleIntent(Intent? intent)
        {
            if (intent == null)
                return;

            if (!intent.HasExtra("testId"))
                return;

            string? testId = intent.GetStringExtra("testId");

            if (string.IsNullOrWhiteSpace(testId))
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await OpenOrUpdateTestPage(testId);
            });
        }

        private void RequestNotificationPermissionIfNeeded()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications)
                    != Permission.Granted)
                {
                    RequestPermissions(
                        new[] { Android.Manifest.Permission.PostNotifications },
                        1001
                    );
                }
            }
        }

        private async Task OpenOrUpdateTestPage(string testId)
        {
            var nav = Shell.Current?.Navigation;
            if (nav == null)
                return;

            // Ищем существующую страницу
            var existingPage = nav.NavigationStack
                .OfType<TestRunPage>()
                .FirstOrDefault();

            if (existingPage != null)
            {
                // Обновляем данные
                existingPage.UpdateTest(testId);

                // Возвращаемся к ней
                await nav.PopToRootAsync(false);
                await nav.PushAsync(existingPage, false);
                return;
            }

            // Если страницы нет — создаём новую
            await nav.PushAsync(new TestRunPage(testId));
        }

    }
}