using BhugolLab.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace BhugolLab.Web.Services;

public sealed class EfLeadStore(AppDbContext dbContext) : ILeadStore
{
    public async Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        dbContext.Leads.Add(lead);
        await dbContext.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task<IReadOnlyList<Lead>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Leads
            .AsNoTracking()
            .OrderByDescending(lead => lead.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Lead?> FindAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Leads.FindAsync([id], cancellationToken);

    public async Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var lead = await dbContext.Leads.FindAsync([id], cancellationToken);
        if (lead is null)
        {
            return false;
        }

        lead.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
