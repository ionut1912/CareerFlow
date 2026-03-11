using System.Diagnostics;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.OpenAi;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services.OpenAi;

public sealed class LoggingCompletionService : IAICompletionService
{
    private readonly IAICompletionService _inner;
    private readonly ILogger<LoggingCompletionService> _logger;

    public LoggingCompletionService(
        IAICompletionService inner,
        ILogger<LoggingCompletionService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<CompletionResult> CompleteAsync(
        CompletionRequest request,
        CancellationToken ct = default)
    {
        using var scope = _logger.BeginScope(new
        {
            Model = request.Model,
            MaxTokens = request.MaxTokens
        });

        _logger.LogInformation("Sending completion request to OpenAI");
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await _inner.CompleteAsync(request, ct);

            _logger.LogInformation(
                "Completion succeeded in {ElapsedMs}ms | Tokens used: {Tokens} | Finish: {Reason}",
                sw.ElapsedMilliseconds, result.TokensUsed, result.FinishReason);

            return result;
        }
        catch (OpenAIException ex)
        {
            _logger.LogError(ex,
                "Completion failed after {ElapsedMs}ms with status {StatusCode}",
                sw.ElapsedMilliseconds, ex.StatusCode);
            throw;
        }
    }
}