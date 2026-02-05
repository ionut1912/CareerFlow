using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CarrerFlow.Core.Infrastructure.Persistance.Repositories;

public class AccountRepository(DbSet<Account> dbSet) : GenericRepository<Account>(dbSet), IAccountRepository
{
    public async Task<Account?> GetAccountByUsernameAsync(string username, CancellationToken cancellationToken)
    {

        return await dbSet.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }
}
