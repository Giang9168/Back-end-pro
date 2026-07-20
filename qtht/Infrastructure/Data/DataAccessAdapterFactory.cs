using SD.LLBLGen.Pro.ORMSupportClasses;
using Qtht.Data.DatabaseSpecific;

namespace Infrastructure.Data;

/// <summary>
/// Hiện thực factory: tạo DataAccessAdapter (Postgres) với connection string lấy từ cấu hình.
/// </summary>
public sealed class DataAccessAdapterFactory : IDataAccessAdapterFactory
{
    private readonly string _connectionString;

    public DataAccessAdapterFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDataAccessAdapter Create() => new DataAccessAdapter(_connectionString);
}
