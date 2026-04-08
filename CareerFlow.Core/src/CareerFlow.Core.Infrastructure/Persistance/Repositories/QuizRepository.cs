using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class QuizRepository(DbSet<QuizQuestion> dbSet) : GenericRepository<QuizQuestion>(dbSet), IQuizRepository
{
}