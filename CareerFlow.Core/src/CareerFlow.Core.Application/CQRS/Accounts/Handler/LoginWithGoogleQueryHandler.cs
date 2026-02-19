using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class LoginWithGoogleQueryHandler
{
    private readonly IAuthService _authService;
    private readonly ITokenService _jwtTokenService;
    private readonly ILogger<LoginWithGoogleQueryHandler> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginWithGoogleQueryHandler(IAuthService authService, IRefreshTokenRepository refreshTokenRepository,
        ITokenService jwtTokenService, IUnitOfWork unitOfWork, ILogger<LoginWithGoogleQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(authService);
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(jwtTokenService);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);
        _authService = authService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(LoginWithGoogleQuery request, CancellationToken cancellationToken)
    {
        var googleUser = await _authService.LoginWithGoogleAsync(request.IdToken, cancellationToken);
        var token = _jwtTokenService.GenerateToken(googleUser);
        var refresh = _jwtTokenService.GenerateRefreshToken(googleUser.Id, token.Token);
        await _refreshTokenRepository.AddAsync(refresh);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User-ul cu email-ul {Email} s-a logat cu succes utilizand google", googleUser.Email);
        return googleUser.ToAccountDto(token.Token, refresh.Token);
    }
}