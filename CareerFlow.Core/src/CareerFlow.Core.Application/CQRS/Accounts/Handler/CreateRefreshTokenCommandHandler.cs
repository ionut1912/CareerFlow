using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class CreateRefreshTokenCommandHandler
{
    private readonly ILogger<CreateRefreshTokenCommandHandler> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRefreshTokenCommandHandler(ILogger<CreateRefreshTokenCommandHandler> logger, IAccountRepository accountRepository, IJwtTokenService jwtTokenService, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(refreshTokenRepository, nameof(refreshTokenRepository));
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(jwtTokenService, nameof(jwtTokenService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
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
            _logger.LogInformation("Refresh token already exists for token: {Token}", request.Token);
            throw new InvalidRefreshTokenException("Token invalid");
        }
        if (storedToken.IsUsed)
        {
            _logger.LogInformation("Refresh token already used for token: {Token}", request.Token);
            throw new TokenAlreadyUsedExcception("Token already used");
        }
        if (storedToken.IsRevoked)
        {
            _logger.LogInformation("Refresh token already revoked for token: {Token}", request.Token);
            throw new TokenRevokedException("Token revoked");
        }
        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            _logger.LogInformation("Refresh token expired for token: {Token}", request.Token);
            throw new TokenExpiredException("Token expired");
        }
        storedToken.MarkAsUsed();
        _refreshTokenRepository.Update(storedToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var user = await _accountRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogInformation("User not found for token: {Token}", request.Token);
            throw new AccountNotFoundException($"Contul cu id-ul {storedToken.UserId} nu a fost gasit");
        }

        var newJwtToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id, newJwtToken.Jti);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token created successfully for user: {UserId}", user.Id);
        return new RefreshTokenDto(newJwtToken.Token, newRefreshToken.Token);

    }
}
