namespace CareerFlow.Core.Domain.Models.Assembly;

public sealed record SubchapterAssemblyModel(
    string Title,
    string ContentSummary,
    string TheoryHtml,
    List<QuizItemAssemblyModel> Quiz);