using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class LoginWithLinkedinQueryHandler
{
    private readonly IAuthService _authService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginWithLinkedinQueryHandler> _logger;

    public LoginWithLinkedinQueryHandler(IAuthService authService, IRefreshTokenRepository refreshTokenRepository, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork, ILogger<LoginWithLinkedinQueryHandler> logger)
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

    public async Task<AccountDto> Handle(LoginWithLinkedinQuery request, CancellationToken cancellationToken)
    {
        var linkedinUser = await _authService.LoginWithLinkedInAsync(request.AuthorizationCode,cancellationToken);
        var jwtToken = _jwtTokenService.GenerateToken(linkedinUser);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(linkedinUser.Id, jwtToken.Jti);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {Email} logged in with LinkedIn", linkedinUser.Email);
        return linkedinUser.ToAccountDto(jwtToken.Token, refreshToken.Token);
    }
}
