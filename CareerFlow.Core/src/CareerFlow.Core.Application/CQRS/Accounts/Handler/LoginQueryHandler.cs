using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handler;

public class LoginQueryHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginQueryHandler> _logger;

    public LoginQueryHandler(IAccountRepository accountRepository,
        IPasswordService passwordService,
        ITokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<LoginQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(passwordService, nameof(passwordService));
        ArgumentNullException.ThrowIfNull(jwtTokenService, nameof(jwtTokenService));
        ArgumentNullException.ThrowIfNull(refreshTokenRepository, nameof(refreshTokenRepository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);

        if (account is null)
        {
            _logger.LogError("Procesul de logare a esuat,deoarece contul cu Email :{Email} nu a fost gasit", request.Email);
            throw new AccountNotFoundException($"Contul cu email {request.Email} nu a fost gasit");
        }

        var isPasswordValid = _passwordService.VerifyPassword(request.Password, account.Password);

        if (!isPasswordValid)
        {
            _logger.LogError("Nu ne putem loga,deoarece parola introdusa si cea existenta nu corespund");
            throw new PasswordNotMatchException("Parola existenta si cea introdusa nu corespund");

        }
        var jwtToken = _jwtTokenService.GenerateToken(account);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(account.Id, jwtToken.Token);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var accountDto = account.ToAccountDto(jwtToken.Token, refreshToken.Token);

        _logger.LogInformation("Procesul de logare realizat cu succes");
        return accountDto;
    }
}