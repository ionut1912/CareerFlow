using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CarrerFlow.Core.Infrastructure.Persistance.Repositories;

public class PrivacyPolicyService(DbSet<PrivacyPolicy> dbSet) : GenericRepository<PrivacyPolicy>(dbSet), IPrivacyPolicyService
{
}
