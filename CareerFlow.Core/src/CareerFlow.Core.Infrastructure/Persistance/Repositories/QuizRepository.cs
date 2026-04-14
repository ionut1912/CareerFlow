using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class QuizRepository(DbSet<QuizQuestion> quizQuestions)
    : GenericRepository<QuizQuestion>(quizQuestions), IQuizRepository
{
    private readonly DbSet<QuizQuestion> _quizQuestions = quizQuestions;

    public async Task AddRangeAsync(List<QuizQuestion> quizQuestions, CancellationToken cancellationToken)
    {
        await _quizQuestions.AddRangeAsync(quizQuestions, cancellationToken);
    }
}