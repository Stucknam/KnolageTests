namespace KnolageTests.Helpers;

public static class ServiceHelper
{
    public static T GetService<T>() =>
        Current.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T)} not found");

    public static IServiceProvider Current =>
        MauiProgram.Services;
}