using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Users.Queries.GetAll;

// Query – đọc danh sách user (đọc DB qua LLBLGen ở tầng Infrastructure)
public record GetUsersQuery : IRequest<List<UserDto>>;

public record UserDto(string Id, string? RoleId, string? UserName);

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

        // Map entity LLBLGen -> DTO để không lộ kiểu ORM ra ngoài API
        return users.Select(u => new UserDto(u.Id, u.RoleId, u.UserName)).ToList();
    }
}
