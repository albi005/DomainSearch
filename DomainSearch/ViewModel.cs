using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Application = FlaUI.Core.Application;
using Window = FlaUI.Core.AutomationElements.Window;

namespace DomainSearch;

public partial class ViewModel : ObservableObject
{
    [ObservableProperty] private string? _domain;
    [ObservableProperty] private string? _registrar;

    public ViewModel()
    {
        Main();
    }

    private async void Main()
    {
        while (true)
        {
            await Task.Delay(100);
            UpdateUrls();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void UpdateUrls()
    {
        Process process = Process.GetProcessesByName("chrome").First();
        UIA3Automation automation = new();
        Application application = Application.Attach(process);
        Window window = application.GetMainWindow(automation);
        AutomationElement? searchBar = window.FindFirstDescendant(c => c.ByName("Address and search bar"));
        string url = "https://" + searchBar?.Patterns.Value.Pattern.Value.Value;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) return;

        if (uri.Authority == "domainr.com")
        {
            if (uri.AbsolutePath.Length < 2) return;
            Domain = uri.AbsolutePath[1..];
        }

        Registrar = uri.Authority;
    }
}