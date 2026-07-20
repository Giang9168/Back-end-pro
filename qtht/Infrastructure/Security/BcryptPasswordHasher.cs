using Application.Common.Interfaces;

namespace Infrastructure.Security;

/// <summary>
/// Băm mật khẩu bằng BCrypt.
///
/// BCrypt tự sinh salt ngẫu nhiên và nhúng luôn vào chuỗi kết quả, nên chỉ cần
/// một cột duy nhất trong DB. Định dạng: $2a$12$&lt;22 ký tự salt&gt;&lt;31 ký tự hash&gt;
///
/// Work factor 12 = 2^12 vòng lặp, mất khoảng 250ms mỗi lần băm. Chậm là CỐ Ý:
/// nó khiến việc dò mật khẩu hàng loạt trở nên bất khả thi. Đừng hạ xuống cho "nhanh".
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        // Hash trong DB có thể hỏng/rỗng (dữ liệu cũ, nhập tay...) — BCrypt sẽ ném
        // SaltParseException. Nuốt lỗi và coi như sai mật khẩu, đừng để vỡ thành 500.
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
