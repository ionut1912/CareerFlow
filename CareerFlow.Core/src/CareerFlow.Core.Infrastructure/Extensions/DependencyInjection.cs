using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Gateways;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using CareerFlow.Core.Infrastructure.Persistance;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using InfisicalConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infra.Extensions;

namespace CareerFlow.Core.Infrastructure.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        private IServiceCollection AddDatabaseConfiguration(IConfiguration configuration)
        {
            services.AddDatabase<ApplicationDbContext>(configuration);
            return services;
        }

        private IServiceCollection AddRepositories()
        {
            services
                .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
                .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
                .AddRepository<RefreshToken, RefreshTokenRepository, IRefreshTokenRepository, ApplicationDbContext>()
                .AddRepository<UserProfile, UserProfileRepository, IUserProfileRepository, ApplicationDbContext>()
                .AddRepos<ITokenService, TokenService>()
                .AddRepos<IPasswordService, PasswordService>()
                .AddRepos<IAuthService, AuthService>()
                .AddRepos<IUnitOfWork, UnitOfWork>()
                .AddRepos<IEmailService, EmailService>()
                .AddRepos<IGoogleTokenValidator, GoogleTokenValidator>()
                .AddRepos<IMailClient, PostmarkMailClient>()
                .AddRepos<ISocialService, SocialService>()
                .AddRepos<ILegalService, LegalService>();

            return services;
        }

        private IServiceCollection AddInfisical(IConfiguration configuration,
            string environment)
        {
            var infisicalClientId = configuration["Infisical:ClientId"];
            var infisicalClientSecret = configuration["Infisical:ClientSecret"];
            var infisicalProjectId = configuration["Infisical:ProjectId"];

            if (string.IsNullOrWhiteSpace(infisicalClientId) ||
                string.IsNullOrWhiteSpace(infisicalProjectId) ||
                string.IsNullOrWhiteSpace(infisicalClientSecret)) return services;
            if (configuration is IConfigurationManager configManager)
                configManager.AddInfisical(new InfisicalConfigBuilder()
                    .SetProjectId(infisicalProjectId)
                    .SetEnvironment(environment)
                    .SetAuth(new InfisicalAuthBuilder()
                        .SetUniversalAuth(infisicalClientId, infisicalClientSecret)
                        .Build())
                    .Build());

            return services;
        }

        private IServiceCollection AddSettings(IConfiguration configuration)
        {
            services.AddMemoryCache()
                .Configure<SocialAuthSettings>(configuration.GetSection(SocialAuthSettings.SectionName))
                .Configure<SocialAuthSettings>(configuration.GetSection(SocialAuthSettings.SectionName))
                .Configure<PostmarkSettings>(configuration.GetSection(PostmarkSettings.SectionName))
                .Configure<LegalDocSettings>(configuration.GetSection(LegalDocSettings.SectionName))
                .AddHttpClient<IAuthService, AuthService>();

            services.AddHttpClient<IGithubPagesRequestsSender, GithubPagesRequestsSender>();
            return services;
        }


        private IServiceCollection AddHangfireConfiguration(IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(opts =>
                    opts.UseNpgsqlConnection(
                        configuration.GetConnectionString("DefaultConnection"))));

            services.AddHangfireServer(opts =>
            {
                opts.WorkerCount = 4;
                opts.Queues = ["default"];
            });

            services.AddScoped<LegalDocumentCheckerJob>();
            return services;
        }


        public IServiceCollection AddInfrastructure(IConfiguration configuration, string environment)
        {
            services
                .AddHangfireConfiguration(configuration)
                .AddInfisical(configuration, environment)
                .AddSettings(configuration)
                .AddDatabaseConfiguration(configuration)
                .AddRepositories();
            return services;
        }
    }
}