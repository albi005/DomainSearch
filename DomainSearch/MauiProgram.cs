namespace DomainSearch;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .Build();
}

public class App : Application
{
    public App()
    {
        MainPage = new MainPage();
    }
}