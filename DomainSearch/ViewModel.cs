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
    private readonly string[] _possibleDomains;
    private readonly Db _db = new();
    private readonly AutomationElement _automationElement;

    private string? _currentDomain;
    private string? _registrar;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private Domain? _domain;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private Offer? _offer;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _dollarsPerYear = "";

    public ViewModel()
    {
        string[] domains =
        {
            "albi",
            "alb1",
            "albert",
            "albrt",
            "albertrn"
        };
        string[] tlds = { "io", "hu", "dev", "me", "gg", "net", "com" };

        _possibleDomains = new string[domains.Length * tlds.Length];
        for (int i = 0; i < domains.Length; i++)
        {
            string domain = domains[i];
            for (int j = 0; j < tlds.Length; j++)
            {
                string tld = tlds[j];
                _possibleDomains[i * tlds.Length + j] = domain + "." + tld;
            }
        }

        Process process = Process.GetProcessesByName("chrome").First();
        UIA3Automation automation = new();
        Application application = Application.Attach(process);
        Window window = application.GetMainWindow(automation);
        _automationElement = window.FindFirstDescendant(c => c.ByName("Address and search bar"));

        Main();
    }

    public ObservableCollection<Offer> Offers { get; } = new();
    public ObservableCollection<string> NotChecked { get; } = new();
    public ObservableCollection<string> Checked { get; } = new();
    public ObservableCollection<Stat> RegistrarStats { get; } = new();

    private async void Main()
    {
        await _db.Database.MigrateAsync();
        await UpdateChecked();

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
            if (domain == _currentDomain) return;
            _currentDomain = domain;
            await UpdateDomain();
            await UpdateOffers();
            await UpdateRegistrarStats();
            await UpdateOffer();
        }
        
        if (_registrar != uri.Authority)
        {
            _registrar = uri.Authority;
            await UpdateOffer();
        }
    }

    private async Task UpdateOffers()
    {
        Offers.Clear();

        if (Domain == null) return;

        Offer[] offers = await _db.Offers
            .Where(o => o.DomainId == Domain.Id)
            .OrderBy(o => o.DollarsPerYear)
            .ToArrayAsync();
        foreach (Offer offer in offers) Offers.Add(offer);
    }

    public bool CanSave()
    {
        if (Domain == null) return false;
        return Offer == null || float.TryParse(DollarsPerYear, out _);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (Offer != null)
        {
            float dollarsPerYear = float.Parse(DollarsPerYear);
            Offer!.DollarsPerYear = dollarsPerYear;
            if (!_db.Offers.Contains(Offer))
                _db.Add(Offer);
        }

        if (!_db.Domains.Contains(Domain))
            _db.Add(Domain!);

        await _db.SaveChangesAsync();
        await UpdateOffers();
        await UpdateChecked();
    }

    private async Task UpdateDomain()
    {
        Domain = _currentDomain != null
            ? await _db.Domains.FindAsync(_currentDomain) ?? new(_currentDomain, false)
            : null;
    }

    private async Task UpdateOffer()
    {
        if (_currentDomain == null || _registrar == null || _registrar.Contains("domainr"))
        {
            Offer = null;
            DollarsPerYear = "";
            return;
        }

        Offer = await _db.Offers.FindAsync(_currentDomain, _registrar);

        if (Offer == null)
        {
            Offer = new(_currentDomain, _registrar, 0, "");
            DollarsPerYear = "";
        }
        else
            DollarsPerYear = Offer.DollarsPerYear.ToString(CultureInfo.InvariantCulture);
    }

    private async Task UpdateChecked()
    {
        NotChecked.Clear();
        Checked.Clear();

        List<string> @checked = await _db.Domains.OrderBy(d => d.Id).Select(d => d.Id).ToListAsync();
        foreach (string possibleDomain in _possibleDomains)
        {
            if (@checked.BinarySearch(possibleDomain) < 0)
                NotChecked.Add(possibleDomain);
            else
                Checked.Add(possibleDomain);
        }
    }

    private async Task UpdateRegistrarStats()
    {
        RegistrarStats.Clear();
        if (string.IsNullOrWhiteSpace(_currentDomain) || !_currentDomain.Contains('.')) return;

        string tld = '.' + _currentDomain.Split('.')[1];
        List<Stat> stats = await _db.Offers
            .Where(o => o.DomainId.EndsWith(tld))
            .GroupBy(o => o.Registrar)
            .OrderBy(g => g.Min(o => o.DollarsPerYear))
            .Select(g => new Stat(
                g.Key,
                g.Min(o => o.DollarsPerYear),
                g.Max(o => o.DollarsPerYear),
                g.Average(o => o.DollarsPerYear)
            ))
            .ToListAsync();
        foreach (Stat stat in stats)
        {
            RegistrarStats.Add(stat);
        }
    }
}

public record Stat(string Registrar, float Min, float Max, float Avg);