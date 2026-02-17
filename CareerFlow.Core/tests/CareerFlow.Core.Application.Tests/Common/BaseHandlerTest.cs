using Microsoft.Extensions.Logging;
using Moq;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Tests.Common;

public abstract class BaseHandlerTest<THandler>
{
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly Mock<ILogger<THandler>> _loggerMock;

    protected BaseHandlerTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<THandler>>();
    }

    protected CancellationToken Ct => CancellationToken.None;
}