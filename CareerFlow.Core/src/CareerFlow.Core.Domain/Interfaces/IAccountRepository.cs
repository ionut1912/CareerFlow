using CareerFlow.Core.Domain.Entities;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Interfaces;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<Account?> GetAccountByUsernameAsync(string username, CancellationToken cancellationToken);
}