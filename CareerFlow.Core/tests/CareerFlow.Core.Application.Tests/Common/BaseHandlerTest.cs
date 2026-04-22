using Microsoft.Extensions.Logging;
using Moq;

namespace CareerFlow.Core.Application.Tests.Common;

public abstract class BaseHandlerTest<THandler>
{
    protected readonly Mock<ILogger<THandler>> LoggerMock;
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock;

    protected BaseHandlerTest()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        LoggerMock = new Mock<ILogger<THandler>>();
    }

    protected CancellationToken Ct => CancellationToken.None;
}