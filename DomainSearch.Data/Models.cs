using Microsoft.EntityFrameworkCore;

namespace DomainSearch.Data;

public class Db : DbContext
{
    public DbSet<Domain> Domains => Set<Domain>();
    public DbSet<Offer> Offers => Set<Offer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(@"Data Source=C:\Users\ragan\OneDrive\Programming\domains.sqlite");
    }
}

public class Domain
{
    public Domain(string id, bool isAvailable)
    {
        Id = id;
        IsAvailable = isAvailable;
    }

    public string Id { get; init; }
    public bool IsAvailable { get; set; }
}

[PrimaryKey(nameof(DomainId), nameof(Registrar))]
public class Offer
{
    public Offer(string domainId, string registrar, float dollarsPerYear, string notes)
    {
        DomainId = domainId;
        DollarsPerYear = dollarsPerYear;
        Registrar = registrar;
        Notes = notes;
    }

    public Domain? Domain { get; set; }
    public string DomainId { get; init; }
    public string Registrar { get; init; }
    public float DollarsPerYear { get; set; }
    public string Notes { get; set; }
}