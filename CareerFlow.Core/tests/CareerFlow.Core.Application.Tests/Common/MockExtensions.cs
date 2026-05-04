using Microsoft.Extensions.Logging;

using Moq;

namespace CareerFlow.Core.Application.Tests.Common;

public static class MockExtensions
{
    public static void VerifyLogError<T>(this Mock<ILogger<T>> logger, string expectedMessagePart, Times times)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessagePart)),
                It.IsAny<Exception?>(), // Allow null exception
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), // Allow null exception in the formatter
            times);
    }

    public static void VerifySaveChanges(this Mock<IUnitOfWork> unitOfWork, Times times) =>
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times);
}
