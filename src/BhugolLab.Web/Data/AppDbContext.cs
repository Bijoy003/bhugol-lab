using Microsoft.EntityFrameworkCore;

namespace BhugolLab.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Explicit max lengths keep the SQL Server column types (nvarchar(N)) parseable
        // by SQLite, so one migration set applies on both providers.
        builder.Entity<Lead>(lead =>
        {
            lead.HasKey(l => l.Id);
            lead.Property(l => l.Name).HasMaxLength(100).IsRequired();
            lead.Property(l => l.Email).HasMaxLength(254).IsRequired();
            lead.Property(l => l.Company).HasMaxLength(100);
            lead.Property(l => l.Message).HasMaxLength(4000).IsRequired();
            lead.Property(l => l.Source).HasMaxLength(500);
            lead.Property(l => l.Referrer).HasMaxLength(500);
            lead.Property(l => l.Status).HasMaxLength(30).IsRequired().HasDefaultValue(LeadStatus.New);
            lead.HasIndex(l => l.CreatedAt);
        });
    }
}
