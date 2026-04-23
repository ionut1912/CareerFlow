namespace CareerFlow.Core.Domain.Models.Assembly;

public sealed record QuizItemAssemblyModel(string Question, List<QuizOptionAssemblyModel> Options);
