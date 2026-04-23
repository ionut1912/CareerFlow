using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Setup;

internal sealed class FakeLogger<T> : ILogger<T>
{
    private readonly List<FakeLogRecord> _records = [];

    public IReadOnlyList<FakeLogRecord> Records => _records;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        _records.Add(new FakeLogRecord(logLevel, formatter(state, exception)));

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;


    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

internal sealed record FakeLogRecord(LogLevel Level, string Message);
