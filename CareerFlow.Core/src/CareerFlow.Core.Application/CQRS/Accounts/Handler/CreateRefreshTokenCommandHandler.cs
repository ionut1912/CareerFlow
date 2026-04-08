using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class CreateRefreshTokenCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenService _jwtTokenService;
    private readonly ILogger<CreateRefreshTokenCommandHandler> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRefreshTokenCommandHandler(ILogger<CreateRefreshTokenCommandHandler> logger,
        IAccountRepository accountRepository, ITokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(jwtTokenService);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _logger = logger;
        _refreshTokenRepository = refreshTokenRepository;
        _accountRepository = accountRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshTokenDto> Handle(CreateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.GetExistingTokenAsync(request.RefreshToken, cancellationToken);
        if (storedToken is null)
        {
            _logger.LogError("Refresh token-ul {RefreshToken} nu exista pt token-ul:{Token}", request.RefreshToken,
                request.Token);
            throw new InvalidRefreshTokenException($"Token-ul {request.RefreshToken} este invalid");
        }

        if (storedToken.IsUsed)
        {
            _logger.LogError("Refresh token-ul {RefreshToken} a fost deja folosit pt token-ul: {Token}",
                request.RefreshToken, request.Token);
            throw new TokenAlreadyUsedExcception(
                $"Refresh token-ul {request.RefreshToken} a fost folosit pt token-ul {request.Token}");
        }

        if (storedToken.IsRevoked)
        {
            _logger.LogError("Refresh token-ul {RefreshToken} a fost revocat pentru token-ul: {Token}",
                request.RefreshToken, request.Token);
            throw new TokenRevokedException(
                $"Token-ul {request.RefreshToken} a fost revocat pentru token-ul {request.Token}");
        }

        var user = await _accountRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogError("User-ul cu id-ul {Id} nu a fost gasit", storedToken.UserId);
            throw new AccountNotFoundException($"User-ul cu id-ul {storedToken.UserId} nu a fost gasit");
        }

        storedToken.MarkAsUsed();
        storedToken.MarkAsRevoked();
        _refreshTokenRepository.Update(storedToken);

        var newJwtToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id, newJwtToken.Token);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token-ul a fost creat cu succes pentru user-ul cu id-ul {Id}", user.Id);
        return new RefreshTokenDto(newJwtToken.Token, newRefreshToken.TokenHash);
    }
}