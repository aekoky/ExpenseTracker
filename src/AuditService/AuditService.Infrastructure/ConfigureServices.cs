﻿using AuditService.Application.Account.GetAccountById;
using AuditService.Domain.Account;
using AuditService.Infrastructure.Account;
using AuditService.Infrastructure.Common;
using AuditService.Infrastructure.Transaction;
using JasperFx;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.RabbitMQ;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "ExpenseTracker_";
        });
        services.AddMarten(options =>
        {
            // Establish the connection string to your Marten database
            options.Connection(configuration.GetConnectionString("Marten")!);

            // Specify that we want to use STJ as our serializer
            options.UseSystemTextJsonForSerialization();
            // If we're running in development mode, let Marten just take care
            // of all necessary schema building and patching behind the scenes
            if (hostEnvironment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
            options.Projections.Add<AccountProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Async);
            options.Projections.Add<TransactionProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Async);

        })
            .AddAsyncDaemon(DaemonMode.Solo)
            .UseLightweightSessions();
        services.AddScoped<IAccountQueryRepository, AccountQueryRepository>();
        return services;
    }
    public static IHostBuilder AddInfrastructureHostBuilder(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseWolverine(options =>
        {
            options.UseFluentValidation();
            options.Services.AddSingleton(typeof(IFailureAction<>), typeof(ValidationFailureAction<>));
            options.Discovery.IncludeAssembly(typeof(GetAccountByIdQuery).Assembly);
            options.UseRabbitMqUsingNamedConnection("RabbitMQ")
             .AutoProvision();

            options.ListenToRabbitQueue("Account", q =>
            {
                q.PurgeOnStartup = true;
                q.TimeToLive(TimeSpan.FromMinutes(5));
            });
            options.Policies.UseDurableInboxOnAllListeners();
        });

        return hostBuilder;
    }
}
