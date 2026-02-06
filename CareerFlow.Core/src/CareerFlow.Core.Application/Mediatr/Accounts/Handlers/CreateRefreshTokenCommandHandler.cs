using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class CreateRefreshTokenCommandHandler : IRequestHandler<CreateRefreshTokenCommand, RefreshTokenDto>
{
    private readonly ILogger<CreateRefreshTokenCommandHandler> _logger;
    private readonly IRefreshTokenRepository _refreshTokenService;
    private readonly IAccountRepository _accountRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRefreshTokenCommandHandler(ILogger<CreateRefreshTokenCommandHandler> logger,IAccountRepository accountRepository,IJwtTokenService jwtTokenService, IRefreshTokenRepository refreshTokenService,IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(refreshTokenService, nameof(refreshTokenService));
            ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(jwtTokenService, nameof(jwtTokenService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _refreshTokenService = refreshTokenService;
        _accountRepository = accountRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshTokenDto> Handle(CreateRefreshTokenCommand request, CancellationToken cancellationToken = default)
    { 
      var storedToken=await _refreshTokenService.GetExistingTokenAsync(request.RefreshToken,cancellationToken);
       if(storedToken is  null)
        {
            _logger.LogInformation("Refresh token already exists for token: {Token}", request.Token);
            throw new InvalidRefreshTokenException("Token invalid");
        }
        if (storedToken.IsUsed) { 
            _logger.LogInformation("Refresh token already used for token: {Token}", request.Token);
            throw new TokenAlreadyUsedExcception("Token already used");
        }
        if(storedToken.IsRevoked)
        {
            _logger.LogInformation("Refresh token already revoked for token: {Token}", request.Token);
            throw new TokenRevokedException("Token revoked");
        }
        if(storedToken.ExpiryDate < DateTime.UtcNow)
        {
            _logger.LogInformation("Refresh token expired for token: {Token}", request.Token);
            throw new TokenExpiredException("Token expired");
        }
        storedToken.MarkAsUsed();
        _refreshTokenService.Update(storedToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var user=await _accountRepository.GetByIdAsync(storedToken.UserId,cancellationToken);
        if(user is null)
        {
            _logger.LogInformation("User not found for token: {Token}", request.Token);
            throw new AccountNotFoundException($"Contul cu id-ul {storedToken.UserId} nu a fost gasit");
        }

        var newJwtToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id,newJwtToken.Jti);
        await _refreshTokenService.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token created successfully for user: {UserId}", user.Id);
        return new RefreshTokenDto(newJwtToken.Token, newRefreshToken.Token);

    }
}
