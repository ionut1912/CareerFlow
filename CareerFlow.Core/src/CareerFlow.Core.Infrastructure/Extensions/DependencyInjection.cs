<<<<<<< HEAD
using Amazon.Runtime;
using Amazon.S3;
=======
>>>>>>> master
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
<<<<<<< HEAD
using Microsoft.Extensions.Options;
using Shared.Infra.Extensions;
using StackExchange.Redis;
=======
using Shared.Infra.Extensions;
>>>>>>> master

namespace CareerFlow.Core.Infrastructure.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
<<<<<<< HEAD
        private IServiceCollection AddRedisCache(IConfiguration configuration)
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

=======
>>>>>>> master
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
<<<<<<< HEAD
                .AddRepository<CourseUpload, CourseUploadsRepository, ICourseUploadsRepository, ApplicationDbContext>()
                .AddRepository<CourseJob, CourseJobRepository, ICourseJobRepository, ApplicationDbContext>()
                .AddRepository<Chapter, ChapterRepository, IChapterRepository, ApplicationDbContext>()
                .AddRepository<Course, CourseRepository, ICourseRepository, ApplicationDbContext>()
                .AddRepository<QuizQuestion, QuizRepository, IQuizRepository, ApplicationDbContext>()
=======
>>>>>>> master
                .AddRepos<ITokenService, TokenService>()
                .AddRepos<IPasswordService, PasswordService>()
                .AddRepos<IAuthService, AuthService>()
                .AddRepos<IUnitOfWork, UnitOfWork>()
                .AddRepos<IEmailService, EmailService>()
                .AddRepos<IGoogleTokenValidator, GoogleTokenValidator>()
                .AddRepos<IMailClient, PostmarkMailClient>()
                .AddRepos<ISocialService, SocialService>()
<<<<<<< HEAD
                .AddRepos<ILegalService, LegalService>()
                .AddRepos<ICourseService, CourseService>()
                .AddRepos<ICoursePersistenceService,CoursePersistenceService>();
=======
                .AddRepos<ILegalService, LegalService>();

>>>>>>> master
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
<<<<<<< HEAD
            services.AddHttpClient<IDocumentAnalyzerService, DocsAnalyzerService>();
            services.AddHttpClient<IAnalyzerService, CourseGenerationService>();
            return services;
        }

        private IServiceCollection AddStorageConfiguration(IConfiguration configuration)
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
=======
            return services;
        }

>>>>>>> master

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
<<<<<<< HEAD
            services.AddScoped<ProcessCourseJob>();
            return services;
        }

        private IServiceCollection AddAnalyzerService(IConfiguration configuration)
        {
            services.Configure<AnalyzerSettings>(
                configuration.GetSection(AnalyzerSettings.SectionName));

            services.AddHttpClient<IDocumentAnalyzerService, DocsAnalyzerService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<AnalyzerSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSec);
            });

            return services;
        }
=======
            return services;
        }

>>>>>>> master

        public IServiceCollection AddInfrastructure(IConfiguration configuration, string environment)
        {
            services
                .AddHangfireConfiguration(configuration)
                .AddInfisical(configuration, environment)
                .AddSettings(configuration)
                .AddDatabaseConfiguration(configuration)
<<<<<<< HEAD
                .AddRepositories()
                .AddRedisCache(configuration)
                .AddStorageConfiguration(configuration)
                .AddAnalyzerService(configuration);
=======
                .AddRepositories();
>>>>>>> master
            return services;
        }
    }
}