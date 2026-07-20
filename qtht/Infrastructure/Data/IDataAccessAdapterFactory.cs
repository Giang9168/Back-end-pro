using SD.LLBLGen.Pro.ORMSupportClasses;

namespace Infrastructure.Data;

/// <summary>
/// Tạo mới một <see cref="IDataAccessAdapter"/> (LLBLGen) cho mỗi đơn vị công việc.
/// Adapter KHÔNG thread-safe và ôm sẵn một connection, nên luôn tạo mới rồi
/// <c>Dispose</c> sau khi dùng (dùng <c>using</c>).
/// Interface này nằm ở Infrastructure vì nó là chi tiết hạ tầng — Application
/// chỉ biết tới các repository, không biết ORM nào đang chạy bên dưới.
/// </summary>
public interface IDataAccessAdapterFactory
{
    IDataAccessAdapter Create();
}
