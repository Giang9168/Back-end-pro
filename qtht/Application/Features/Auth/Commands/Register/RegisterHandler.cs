using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private const string DefaultRoleCode = "USER";

    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _hasher;

    public RegisterHandler(IUserRepository users, IRoleRepository roles, IPasswordHasher hasher)
    {
        _users = users;
        _roles = roles;
        _hasher = hasher;
    }

    // Kiểm tra hình thức (rỗng, độ dài, định dạng email) đã do RegisterCommandValidator
    // lo ở ValidationBehaviour. Tới đây dữ liệu chắc chắn hợp lệ về mặt hình thức,
    // chỉ còn những thứ phải hỏi DB mới biết.
    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();

        // Chốt chặn thật là unique index ux_app_user_username trong DB —
        // hai request song song vẫn lọt qua chỗ này. Đây chỉ để trả lỗi đẹp.
        if (await _users.GetByUserNameAsync(userName, cancellationToken) is not null)
        {
            return Result<RegisterResponse>.Failure("Tên đăng nhập đã tồn tại", "REGISTER_USERNAME_TAKEN");
        }

        var role = await ResolveRoleAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return request.RoleId is null
                ? Result<RegisterResponse>.Failure(
                    $"Chưa có vai trò mặc định '{DefaultRoleCode}' trong hệ thống", "REGISTER_DEFAULT_ROLE_MISSING")
                : Result<RegisterResponse>.Failure("Vai trò không tồn tại", "REGISTER_ROLE_NOT_FOUND");
        }

        var user = new User
        {
            Id           = Guid.NewGuid(),
            UserName     = userName,
            Email        = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            PasswordHash = _hasher.Hash(request.Password),   // mật khẩu thô dừng lại ở đây
            RoleId       = role.Id,
            IsActive     = true
        };

        var created = await _users.AddAsync(user, cancellationToken);

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            UserId   = created.Id,
            UserName = created.UserName,
            Email    = created.Email,
            RoleId   = created.RoleId,
            RoleCode = role.Code
        });
    }

    private Task<Role?> ResolveRoleAsync(Guid? roleId, CancellationToken ct)
        => roleId is { } id
            ? _roles.GetByIdAsync(id, ct)
            : _roles.GetByCodeAsync(DefaultRoleCode, ct);
}
