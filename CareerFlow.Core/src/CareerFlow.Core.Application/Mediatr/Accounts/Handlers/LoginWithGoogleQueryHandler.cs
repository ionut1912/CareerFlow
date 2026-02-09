using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class LoginWithGoogleQueryHandler : IRequestHandler<LoginWithGoogleQuery, AccountDto>
{
    private readonly IAuthService _authService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginWithGoogleQueryHandler> _logger;

    public LoginWithGoogleQueryHandler(IAuthService authService, IRefreshTokenRepository refreshTokenRepository, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork, ILogger<LoginWithGoogleQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(authService, nameof(authService));
        ArgumentNullException.ThrowIfNull(refreshTokenRepository, nameof(refreshTokenRepository));
        ArgumentNullException.ThrowIfNull(jwtTokenService, nameof(jwtTokenService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _authService = authService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(LoginWithGoogleQuery request, CancellationToken cancellationToken = default)
    {
        var googleUser = await _authService.LoginWithGoogleAsync(request.IdToken);
        var token = _jwtTokenService.GenerateToken(googleUser);
        var refresh = _jwtTokenService.GenerateRefreshToken(googleUser.Id, token.Jti);
        await _refreshTokenRepository.AddAsync(refresh);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {Email} logged in with Google", googleUser.Email);
        return googleUser.ToAccountDto(token.Token, refresh.Token);
    }
}
