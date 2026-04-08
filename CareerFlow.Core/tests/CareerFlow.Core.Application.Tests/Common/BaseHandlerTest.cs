using Microsoft.Extensions.Logging;
using Moq;

namespace CareerFlow.Core.Application.Tests.Common;

public abstract class BaseHandlerTest<THandler>
{
    protected readonly Mock<ILogger<THandler>> _loggerMock;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;

    protected BaseHandlerTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<THandler>>();
    }

    protected CancellationToken Ct => CancellationToken.None;
}