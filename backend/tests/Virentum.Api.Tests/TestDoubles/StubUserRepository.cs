using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Infrastructure.Persistence.Repositories;

namespace Virentum.Api.Tests.TestDoubles;

/// <summary>Serves one account, or none at all when constructed with null.</summary>
internal sealed class StubUserRepository : IUserRepository
{
    private readonly UserAccount? _user;

    public StubUserRepository(UserAccount? user)
    {
        _user = user;
    }

    public Task<UserAccount?> FindByStoreIdAsync(
        string storeId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_user is not null && _user.StoreId == storeId ? _user : null);
}
