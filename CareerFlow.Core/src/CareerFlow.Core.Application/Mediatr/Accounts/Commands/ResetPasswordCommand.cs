
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Commands;

public record ResetPasswordCommand(string Username, string NewPassword) : IRequest;
