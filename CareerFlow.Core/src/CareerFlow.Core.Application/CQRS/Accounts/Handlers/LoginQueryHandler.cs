using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Authentication;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Accounts.Handlers;

public partial class LoginQueryHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenService _jwtTokenService;
    private readonly ILogger<LoginQueryHandler> _logger;
    private readonly IPasswordService _passwordService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginQueryHandler(IAccountRepository accountRepository,
        IPasswordService passwordService,
        ITokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<LoginQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(passwordService);
        ArgumentNullException.ThrowIfNull(jwtTokenService);
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);
        _accountRepository = accountRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        Account? account = await _accountRepository.GetAccountByEmailAsync(request.Email, cancellationToken);

        if (account is null)
        {
            LogAccountNotFound(request.Email);
            throw new AccountNotFoundException($"Contul cu email {request.Email} nu a fost gasit");
        }

        bool isPasswordValid = _passwordService.VerifyPassword(request.Password, account.Password);

        if (!isPasswordValid)
        {
            LogPasswordNotMatch();
            throw new PasswordNotMatchException("Parola existenta si cea introdusa nu corespund");
        }

        AuthResult jwtToken = _jwtTokenService.GenerateToken(account);
        RefreshToken refreshToken = _jwtTokenService.GenerateRefreshToken(account.Id, jwtToken.Token);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var accountDto = account.ToAccountDto(jwtToken.Token, refreshToken.TokenHash);

        LogLoginSuccess();
        return accountDto;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Procesul de logare a esuat,deoarece contul cu Email :{Email} nu a fost gasit")]
    private partial void LogAccountNotFound(string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Nu ne putem loga,deoarece parola introdusa si cea existenta nu corespund")]
    private partial void LogPasswordNotMatch();

    [LoggerMessage(Level = LogLevel.Information, Message = "Procesul de logare realizat cu succes")]
    private partial void LogLoginSuccess();
}
