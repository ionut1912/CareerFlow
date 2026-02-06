using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CarrerFlow.Core.Infrastructure.Persistance.Repositories;

public class TermAndConditionRepository(DbSet<TermsAndCondition> dbSet) : GenericRepository<TermsAndCondition>(dbSet), ITermsAndConditionsRepository
{
}
