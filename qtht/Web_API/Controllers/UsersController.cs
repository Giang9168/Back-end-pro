using Application.Features.Users.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Lấy danh sách user từ PostgreSQL qua LLBLGen.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _sender.Send(new GetUsersQuery(), ct);
        return Ok(users);
    }
}
