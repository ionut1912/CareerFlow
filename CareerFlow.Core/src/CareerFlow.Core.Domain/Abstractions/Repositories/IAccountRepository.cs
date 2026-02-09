using CareerFlow.Core.Domain.Entities;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<Account?> GetAccountByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<Account?> GetAccountByEmailAsync(string email, CancellationToken cancellationToken);
}