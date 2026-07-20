namespace Application.Common.Interfaces;

/// <summary>
/// Băm và đối chiếu mật khẩu. Application chỉ biết hợp đồng này,
/// không biết đang dùng BCrypt, Argon2 hay gì khác — đổi thuật toán
/// chỉ phải sửa Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Băm mật khẩu thô. Kết quả đã bao gồm salt, cứ lưu thẳng vào DB.</summary>
    string Hash(string password);

    /// <summary>Đối chiếu mật khẩu thô với chuỗi băm đã lưu.</summary>
    bool Verify(string password, string passwordHash);
}
