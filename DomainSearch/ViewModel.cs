using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DomainSearch.Data;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.EntityFrameworkCore;
using Application = FlaUI.Core.Application;
using Window = FlaUI.Core.AutomationElements.Window;

namespace DomainSearch;

public partial class ViewModel : ObservableObject
{
    private readonly Db _db = new();
    private readonly AutomationElement _automationElement;

    [ObservableProperty] private string? _domain;
    [ObservableProperty] private string? _registrar;
    [ObservableProperty] private bool _isAvailable;
    [ObservableProperty] private string? _dollarsPerYear;
    [ObservableProperty] private string _notes = "";

    public ViewModel()
    {
        Process process = Process.GetProcessesByName("chrome").First();
        UIA3Automation automation = new();
        Application application = Application.Attach(process);
        Window window = application.GetMainWindow(automation);
        _automationElement = window.FindFirstDescendant(c => c.ByName("Address and search bar"));

        Main();
    }

    public ObservableCollection<Offer> Offers { get; } = new();

    private async void Main()
    {
        await _db.Database.MigrateAsync();

        while (true)
        {
            await Task.Delay(100);
            await UpdateUrls();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task UpdateUrls()
    {
        string url = "https://" + _automationElement.Patterns.Value.Pattern.Value.Value;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) return;

        if (uri.Authority == "domainr.com")
        {
            if (uri.AbsolutePath.Length < 2) return;
            string domain = uri.AbsolutePath[1..];
            if (domain == Domain) return;
            Domain = domain;
            await UpdateOffers();
            await UpdateCurrent();
            IsAvailable = (await _db.Domains.FindAsync(Domain))?.IsAvailable ?? false;
        }

        if (Registrar != uri.Authority)
        {
            Registrar = uri.Authority;
            await UpdateCurrent();
        }
    }

    private async Task UpdateOffers()
    {
        Offer[] offers =
            await _db.Offers.Where(o => o.DomainId == Domain).OrderBy(o => o.DollarsPerYear).ToArrayAsync();
        Offers.Clear();
        foreach (Offer offer in offers) Offers.Add(offer);
    }

    [RelayCommand]
    private async Task SaveOffer()
    {
        if (string.IsNullOrWhiteSpace(Domain)) return;
        
        Domain? domain = await _db.Domains.FindAsync(Domain);
        if (domain == null)
            _db.Domains.Add(new(Domain, IsAvailable));
        else
            domain.IsAvailable = IsAvailable;
        
        if (!float.TryParse(DollarsPerYear, out float dollarsPerYear)) return;
        if (string.IsNullOrWhiteSpace(Registrar)) return;

        Offer? offer = await _db.Offers.FindAsync(Domain, Registrar);
        if (offer != null)
        {
            offer.DollarsPerYear = dollarsPerYear;
            offer.Notes = Notes;
        }
        else
            _db.Offers.Add(new(Domain, Registrar, dollarsPerYear, Notes));


        await _db.SaveChangesAsync();

        await UpdateOffers();
    }

    private async Task UpdateCurrent()
    {
        if (Registrar == "domainr.com")
        {
            IsAvailable = false;
            DollarsPerYear = "";
            Notes = "";
            return;
        }

        Offer? offer = await _db.Offers.FindAsync(Domain, Registrar);
        
        DollarsPerYear = offer?.DollarsPerYear.ToString(CultureInfo.InvariantCulture);
        Notes = offer?.Notes ?? "";
    }
}