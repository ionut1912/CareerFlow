using System.Net;
using System.Text;
using System.Text.Json;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Gateways.Dtos;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IGoogleTokenValidator> _googleValidatorMock;
    private readonly FakeHttpMessageHandler _handler;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly IOptions<SocialAuthSettings> _settings;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _googleValidatorMock = new Mock<IGoogleTokenValidator>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _settings = Options.Create(new SocialAuthSettings
        {
            Google = new GoogleSettings { ClientId = "google-client-id" },
            LinkedIn = new LinkedInSettings
            {
                ClientId = "li-client-id",
                ClientSecret = "li-secret",
                RedirectUri = "https://app.test/callback"
            }
        });

        _handler = new FakeHttpMessageHandler();
        _httpClient = new HttpClient(_handler);

        _sut = new AuthService(
            _accountRepositoryMock.Object,
            _googleValidatorMock.Object,
            _httpClient,
            _settings,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullAccountRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AuthService(null!, _googleValidatorMock.Object, _httpClient, _settings, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullGoogleValidator_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AuthService(_accountRepositoryMock.Object, null!, _httpClient, _settings, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AuthService(_accountRepositoryMock.Object, _googleValidatorMock.Object, null!, _settings,
                _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AuthService(_accountRepositoryMock.Object, _googleValidatorMock.Object, _httpClient, null!,
                _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AuthService(_accountRepositoryMock.Object, _googleValidatorMock.Object, _httpClient, _settings, null!));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ExistingAccount_ReturnsExistingAccountWithoutCreating()
    {
        // Arrange
        var googleUser = new GoogleUserDto("existing@test.com", "Existing User");

        _googleValidatorMock
            .Setup(x => x.ValidateIdTokenAsync("id-token", "google-client-id"))
            .ReturnsAsync(googleUser);

        var existingAccount = Account.Create("existing@test.com", "pass", "user", "Existing");

        _accountRepositoryMock
            .Setup(x => x.GetAccountByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await _sut.LoginWithGoogleAsync("id-token");

        // Assert
        result.ShouldBe(existingAccount);

        _accountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_NoExistingAccount_CreatesAndSavesNewAccount()
    {
        // Arrange
        _googleValidatorMock
            .Setup(x => x.ValidateIdTokenAsync("id-token", "google-client-id"))
            .ReturnsAsync(new GoogleUserDto("new@test.com", "New User"));

        _accountRepositoryMock
            .Setup(x => x.GetAccountByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _sut.LoginWithGoogleAsync("id-token");

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("new@test.com");

        _accountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_NewAccount_TermsAndPolicyAreAccepted()
    {
        // Arrange
        _googleValidatorMock
            .Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new GoogleUserDto("new2@test.com", "Name"));

        _accountRepositoryMock
            .Setup(x => x.GetAccountByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _sut.LoginWithGoogleAsync("token");

        // Assert
        result.TermsAccepted.ShouldBeTrue();
        result.PrivacyPolicyAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task LoginWithGoogleAsync_NullNameInGoogleUser_UsesFallbackUsername()
    {
        // Arrange
        _googleValidatorMock
            .Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new GoogleUserDto("noname@test.com", null!));

        _accountRepositoryMock
            .Setup(x => x.GetAccountByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _sut.LoginWithGoogleAsync("token");

        // Assert
        result.ShouldNotBeNull();
        result.Username.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ValidatorThrows_PropagatesException()
    {
        // Arrange
        _googleValidatorMock
            .Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        // Act / Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _sut.LoginWithGoogleAsync("bad-token"));
    }

    [Fact]
    public async Task LoginWithLinkedInAsync_ExistingAccount_ReturnsExistingAccountWithoutCreating()
    {
        // Arrange
        SetupLinkedInHttpResponses("li@test.com", "Li User");

        var existing = Account.Create("li@test.com", "pass", "liuser", "Li User");

        _accountRepositoryMock
            .Setup(x => x.GetAccountByEmailAsync("li@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.LoginWithLinkedInAsync("auth-code");

        // Assert
        result.ShouldBe(existing);

        _accountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginWithLinkedInAsync_NewUser_CreatesAndSavesNewAccount()
    {
        // Arrange
        SetupLinkedInHttpResponses("newli@test.com", "New Li");

        _accountRepositoryMock
            .Setup(x => x.GetAccountByEmailAsync("newli@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _sut.LoginWithLinkedInAsync("auth-code");

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("newli@test.com");

        _accountRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginWithLinkedInAsync_TokenEndpointFails_ThrowsHttpRequestException()
    {
        // Arrange
        _handler.Enqueue(new HttpResponseMessage(HttpStatusCode.BadRequest));

        // Act / Assert
        await Should.ThrowAsync<HttpRequestException>(() => _sut.LoginWithLinkedInAsync("bad-code"));
    }

    private void SetupLinkedInHttpResponses(string email, string name)
    {
        // Token response
        _handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { access_token = "fake-access-token" }),
                Encoding.UTF8, "application/json")
        });

        // User info response
        _handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { email, name, sub = "li-sub" }),
                Encoding.UTF8, "application/json")
        });
    }

    // This manual fake is perfectly fine to keep with Moq.
    // It is often cleaner than mocking HttpMessageHandler via Moq's Protected() method.
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();

        public void Enqueue(HttpResponseMessage response)
        {
            _responses.Enqueue(response);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
                throw new InvalidOperationException("No responses queued.");

            return Task.FromResult(_responses.Dequeue());
        }
    }
}