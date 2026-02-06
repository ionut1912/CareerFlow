using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Commands;

public record CreateRefreshTokenCommand(string Token,string RefreshToken) : IRequest<RefreshTokenDto>
{
}
