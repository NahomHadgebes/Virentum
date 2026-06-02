using Virentum.Api.Infrastructure.Persistence.Entities;

namespace Virentum.Api.Infrastructure.Persistence.Repositories;

/// <summary>Abstracts lookup of operator accounts.</summary>
public interface IUserRepository
{
    Task<UserAccount?> FindByStoreIdAsync(string storeId, CancellationToken cancellationToken = default);
}
