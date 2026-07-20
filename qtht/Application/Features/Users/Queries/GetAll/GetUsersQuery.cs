using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Users.Queries.GetAll;

// Query – đọc danh sách user (đọc DB qua LLBLGen ở tầng Infrastructure)
public record GetUsersQuery : IRequest<List<UserDto>>;

// Cố tình KHÔNG có PasswordHash — DTO là cái van chặn field nhạy cảm lọt ra API
public record UserDto(Guid Id, string UserName, string? Email, Guid RoleId, bool IsActive);

public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _users;

    public GetUsersHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        // Map Domain entity -> DTO, chỉ lộ những field client được phép thấy
        return users
            .Select(u => new UserDto(u.Id, u.UserName, u.Email, u.RoleId, u.IsActive))
            .ToList();
    }
}
