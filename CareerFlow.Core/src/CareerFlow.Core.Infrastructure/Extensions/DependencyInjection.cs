using Amazon.Runtime;
using Amazon.S3;
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
using Microsoft.Extensions.Options;
using Shared.Infra.Extensions;
using StackExchange.Redis;

namespace CareerFlow.Core.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
                          .GetSection(CacheSettings.SectionName)
                          .Get<CacheSettings>()
                      ?? throw new InvalidOperationException("Redis configuration is missing.");

        services.Configure<CacheSettings>(
            configuration.GetSection(CacheSettings.SectionName));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { options.ConnectionString },
                AbortOnConnectFail = options.AbortOnConnectFail,
                ConnectRetry = 3,
                ReconnectRetryPolicy = new ExponentialRetry(5000)
            }));

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase<ApplicationDbContext>(configuration);
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
            .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
            .AddRepository<RefreshToken, RefreshTokenRepository, IRefreshTokenRepository, ApplicationDbContext>()
            .AddRepository<UserProfile, UserProfileRepository, IUserProfileRepository, ApplicationDbContext>()
            .AddRepository<CourseUpload, CourseUploadsRepository, ICourseUploadsRepository, ApplicationDbContext>()
            .AddRepository<CourseJob, CourseJobRepository, ICourseJobRepository, ApplicationDbContext>()
            .AddRepository<Chapter, ChapterRepository, IChapterRepository, ApplicationDbContext>()
            .AddRepository<Course, CourseRepository, ICourseRepository, ApplicationDbContext>()
            .AddRepository<QuizQuestion, QuizRepository, IQuizRepository, ApplicationDbContext>()
            .AddRepos<ITokenService, TokenService>()
            .AddRepos<IPasswordService, PasswordService>()
            .AddRepos<IAuthService, AuthService>()
            .AddRepos<IUnitOfWork, UnitOfWork>()
            .AddRepos<IEmailService, EmailService>()
            .AddRepos<IGoogleTokenValidator, GoogleTokenValidator>()
            .AddRepos<IMailClient, PostmarkMailClient>()
            .AddRepos<ISocialService, SocialService>()
            .AddRepos<ILegalService, LegalService>()
            .AddRepos<ICourseService, CourseService>();
        return services;
    }

    public static IServiceCollection AddInfisical(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        string environment)
    {
        var infisicalClientId = configuration["Infisical:ClientId"];
        var infisicalClientSecret = configuration["Infisical:ClientSecret"];
        var infisicalProjectId = configuration["Infisical:ProjectId"];

        if (!string.IsNullOrWhiteSpace(infisicalClientId) &&
            !string.IsNullOrWhiteSpace(infisicalProjectId) &&
            !string.IsNullOrWhiteSpace(infisicalClientSecret))
            if (configuration is IConfigurationManager configManager)
                configManager.AddInfisical(new InfisicalConfigBuilder()
                    .SetProjectId(infisicalProjectId)
                    .SetEnvironment(environment)
                    .SetAuth(new InfisicalAuthBuilder()
                        .SetUniversalAuth(infisicalClientId, infisicalClientSecret)
                        .Build())
                    .Build());

        return serviceCollection;
    }

    public static IServiceCollection AddSettings(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddMemoryCache()
            .Configure<SocialAuthSettings>(configuration.GetSection(SocialAuthSettings.SectionName))
            .Configure<SocialAuthSettings>(configuration.GetSection(SocialAuthSettings.SectionName))
            .Configure<PostmarkSettings>(configuration.GetSection(PostmarkSettings.SectionName))
            .Configure<LegalDocSettings>(configuration.GetSection(LegalDocSettings.SectionName))
            .AddHttpClient<IAuthService, AuthService>();

        serviceCollection.AddHttpClient<IGithubPagesRequestsSender, GithubPagesRequestsSender>();
        return serviceCollection;
    }


    public static IServiceCollection AddStorageConfiguration(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<R2Settings>(configuration.GetSection(R2Settings.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<R2Settings>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = settings.Endpoint,
                ForcePathStyle = true // required for R2
            };

            var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
            return new AmazonS3Client(credentials, config);
        });

        services.AddScoped<IStorageService, R2StorageService>();
        return services;
    }

    private static IServiceCollection AddHangfireConfiguration(
        this IServiceCollection services, IConfiguration configuration)
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
        services.AddScoped<ProcessCourseJob>();
        return services;
    }

    private static IServiceCollection AddAnalyzerService(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AnalyzerSettings>(
            configuration.GetSection(AnalyzerSettings.SectionName));

        services.AddHttpClient<IDocumentAnalyzerService, DocsAnalizerService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<AnalyzerSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSec);
        });

        return services;
    }


    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection,
        IConfiguration configuration, string environment)
    {
        serviceCollection
            .AddHangfireConfiguration(configuration)
            .AddInfisical(configuration, environment)
            .AddSettings(configuration)
            .AddDatabaseConfiguration(configuration)
            .AddRepositories()
            .AddRedisCache(configuration)
            .AddStorageConfiguration(configuration)
            .AddAnalyzerService(configuration);
        return serviceCollection;
    }
}