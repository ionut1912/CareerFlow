using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class AccountRepository(DbSet<Account> dbSet) : GenericRepository<Account>(dbSet), IAccountRepository
{
    private readonly DbSet<Account> _accounts = dbSet;

    public async Task<Account?> GetAccountByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _accounts
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task UpdateTermsAsync(string documentType, CancellationToken cancellationToken)
    {
        switch (documentType)
        {
            case "Terms":
                await _accounts
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(a => a.TermsAccepted, false)
                            .SetProperty(a => a.UpdatedAt, DateTime.UtcNow), cancellationToken);
                break;
            case "Privacy":
                await _accounts
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(a => a.PrivacyPolicyAccepted, false)
                            .SetProperty(a => a.UpdatedAt, DateTime.UtcNow), cancellationToken);
                break;
        }
    }
}
