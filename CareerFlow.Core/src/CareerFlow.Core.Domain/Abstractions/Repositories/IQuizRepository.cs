using CareerFlow.Core.Domain.Entities;

using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface IQuizRepository : IGenericRepository<QuizQuestion>
{
    Task AddRangeAsync(List<QuizQuestion> quizQuestions, CancellationToken cancellationToken);
}
