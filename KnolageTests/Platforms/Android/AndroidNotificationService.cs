using Android.App;
using Android.Content;
using AndroidX.Core.App;
using KnolageTests.Services;
using Application = Android.App.Application;

namespace KnolageTests;

public class AndroidNotificationService : INotificationService
{
    private const string ChannelId = "knowly_channel";

    public AndroidNotificationService()
    {
        CreateNotificationChannel();
    }

    private void CreateNotificationChannel()
    {
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ChannelId,
                "Knowly Notifications",
                NotificationImportance.Default)
            {
                Description = "Notifications from Knowly app"
            };

            var manager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
            manager?.CreateNotificationChannel(channel);
        }
    }

    public void ShowNotification(string title, string message)
    {
        var builder = new NotificationCompat.Builder(Application.Context, ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.ic_stat_notify)
            .SetAutoCancel(true);

        var notification = builder.Build();
        var manager = NotificationManagerCompat.From(Application.Context);
        manager.Notify(new Random().Next(), notification);
        Console.WriteLine("ShowNotification called");

    }

    public void ShowNotification(string title, string message, string testId)
    {
        // Intent для открытия приложения
        var intent = new Intent(Application.Context, typeof(MainActivity));
        intent.PutExtra("testId", testId);
        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntent = PendingIntent.GetActivity(
            Application.Context,
            new Random().Next(),
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
        );

        var builder = new NotificationCompat.Builder(Application.Context, ChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.ic_stat_notify)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent);

        var notification = builder.Build();
        var manager = NotificationManagerCompat.From(Application.Context);
        manager.Notify(new Random().Next(), notification);
    }

}