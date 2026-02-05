using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Handlers;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginQueryHandler> _logger;

    public LoginQueryHandler(IAccountRepository accountRepository, IPasswordService passwordService, IJwtTokenService jwtTokenService,ILogger<LoginQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(passwordService, nameof(passwordService));
        ArgumentNullException.ThrowIfNull(jwtTokenService, nameof(jwtTokenService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepository = accountRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByUsernameAsync(request.Username, cancellationToken);

        if (account is null)
        {
            _logger.LogError("We can't login because account with username :{Username} was not found",request.Username);
            throw new AccountNotFoundException($"Account with {request.Username} not found");
        }

        var isPasswordValid = _passwordService.VerifyPassword(request.Password, account.Password);

        if (!isPasswordValid)
        {
            _logger.LogError("We can t login because the passwords do not match");
            throw new PasswordNotMatchException("Passwords do not match");

        }
            
        var accountDto = account.ToDto(_jwtTokenService.GenerateToken(account));
        _logger.LogInformation("Login successfully");
        return accountDto;
    }
}