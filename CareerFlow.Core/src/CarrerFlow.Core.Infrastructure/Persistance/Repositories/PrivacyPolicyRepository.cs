using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CarrerFlow.Core.Infrastructure.Persistance.Repositories;

public class PrivacyPolicyRepository(DbSet<PrivacyPolicy> dbSet) : GenericRepository<PrivacyPolicy>(dbSet), IPrivacyPolicyRepository
{
}
