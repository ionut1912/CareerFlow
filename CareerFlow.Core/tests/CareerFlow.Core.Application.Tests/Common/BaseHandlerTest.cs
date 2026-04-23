using Microsoft.Extensions.Logging;
using Moq;

namespace CareerFlow.Core.Application.Tests.Common;

public abstract class BaseHandlerTest<THandler>
{
    protected Mock<ILogger<THandler>> LoggerMock { get; } = new();
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();
    protected CancellationToken Ct => CancellationToken.None;

    protected BaseHandlerTest()
    {
        LoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }
}
