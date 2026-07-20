using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Security;
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
        var connectionString = configuration.GetConnectionString("AppDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Thiếu connection string 'AppDb'. Chạy: dotnet user-secrets set \"ConnectionStrings:AppDb\" \"...\" --project qtht/Web_API");
        }

        // LLBLGen trên .NET bắt buộc đăng ký DbProviderFactory (Npgsql) cho DQE PostgreSql (chạy 1 lần lúc khởi động).
        RuntimeConfiguration.ConfigureDQE<PostgreSqlDQEConfiguration>(c => c
            .AddDbProviderFactory(typeof(NpgsqlFactory)));

        // LLBLGen: factory tạo DataAccessAdapter mới cho mỗi unit-of-work.
        // Singleton vì chỉ giữ connection string; adapter thực tế tạo mới mỗi lần Create().
        services.AddSingleton<IDataAccessAdapterFactory>(new DataAccessAdapterFactory(connectionString));

        // Repositories (LLBLGen)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // ── JWT ──────────────────────────────────────────────────────────
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                          ?? new JwtSettings();

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException(
                "Thiếu 'Jwt:SecretKey'. Chạy: dotnet user-secrets set \"Jwt:SecretKey\" \"<chuỗi ngẫu nhiên >= 32 ký tự>\" --project qtht/Web_API");
        }

        // HMAC-SHA256 cần khoá tối thiểu 256 bit. Khoá ngắn hơn sẽ ném exception khó hiểu
        // lúc ký token — chặn ngay ở khởi động cho dễ chẩn đoán.
        if (System.Text.Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException("'Jwt:SecretKey' phải dài ít nhất 32 byte (256 bit).");
        }

        services.AddSingleton(jwtSettings);
        services.AddSingleton<IJwtSettingsProvider>(jwtSettings);
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Băm mật khẩu — không giữ state nên Singleton là đủ
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
