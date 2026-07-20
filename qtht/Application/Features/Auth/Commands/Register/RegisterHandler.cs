using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUserRepository _users;

    public RegisterHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            return Result<RegisterResponse>.Failure("Tên đăng nhập không được để trống", "REGISTER_INVALID_USERNAME");
        }

        // Chặn trùng tên. Lưu ý: đây là kiểm tra "check rồi mới ghi" nên vẫn có
        // khe hở nếu 2 request vào cùng lúc — ràng buộc UNIQUE ở DB mới là chốt chặn thật.
        var existing = await _users.GetByUserNameAsync(userName, cancellationToken);
        if (existing is not null)
        {
            return Result<RegisterResponse>.Failure("Tên đăng nhập đã tồn tại", "REGISTER_USERNAME_TAKEN");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),   // cột id là text, không auto-increment
            UserName = userName,
            RoleId = request.RoleId
        };

        var created = await _users.AddAsync(user, cancellationToken);

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            UserId = created.Id,
            UserName = created.UserName ?? string.Empty,
            RoleId = created.RoleId
        });
    }
}

// TODO khi bảng "user" có thêm cột:
//  1. ALTER TABLE public."user" ADD COLUMN password_hash text, ADD COLUMN email text;
//     ALTER TABLE public."user" ADD CONSTRAINT user_name_unique UNIQUE (user_name);
//  2. Sync catalog + F7 trong LLBLGen, thêm field vào Domain.Entities.User
//  3. RegisterCommand nhận thêm Password + Email
//  4. Băm mật khẩu bằng BCrypt.Net-Next hoặc PasswordHasher<User> trước khi lưu
