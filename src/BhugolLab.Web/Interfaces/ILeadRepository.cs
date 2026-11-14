using BhugolLab.Web.Data.Entities;

namespace BhugolLab.Web.Interfaces;

/// <summary>Persistence for contact-form leads. Implemented by EF Core; swappable in tests.</summary>
public interface ILeadRepository
{
    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);

    /// <summary>All leads, newest first.</summary>
    Task<IReadOnlyList<Lead>> ListAsync(CancellationToken cancellationToken = default);

    Task<Lead?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
}
