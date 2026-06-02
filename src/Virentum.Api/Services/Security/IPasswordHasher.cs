namespace Virentum.Api.Services.Security;

/// <summary>Hashes and verifies passwords. Abstracted so the algorithm can change.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
