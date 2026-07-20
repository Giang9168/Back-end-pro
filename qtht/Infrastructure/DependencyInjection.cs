using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SD.LLBLGen.Pro.DQE.PostgreSql;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException(
                "Thiếu connection string 'AppDb'. Khai báo trong appsettings.json hoặc user-secrets.");

        // LLBLGen trên .NET bắt buộc đăng ký DbProviderFactory (Npgsql) cho DQE PostgreSql (chạy 1 lần lúc khởi động).
        RuntimeConfiguration.ConfigureDQE<PostgreSqlDQEConfiguration>(c => c
            .AddDbProviderFactory(typeof(NpgsqlFactory)));

        // LLBLGen: factory tạo DataAccessAdapter mới cho mỗi unit-of-work.
        // Singleton vì chỉ giữ connection string; adapter thực tế tạo mới mỗi lần Create().
        services.AddSingleton<IDataAccessAdapterFactory>(new DataAccessAdapterFactory(connectionString));

        // Repositories (LLBLGen)
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
