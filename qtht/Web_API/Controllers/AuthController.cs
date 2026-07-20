using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Đăng nhập, trả về JWT.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Data) : Unauthorized(result.ErrorMessage);
    }

    /// <summary>Đăng ký user mới.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        // 409 Conflict cho trường hợp trùng tên, 400 cho dữ liệu không hợp lệ
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "REGISTER_USERNAME_TAKEN"
                ? Conflict(result.ErrorMessage)
                : BadRequest(result.ErrorMessage);
        }

        return CreatedAtAction(nameof(Register), new { id = result.Data!.UserId }, result.Data);
    }
}
