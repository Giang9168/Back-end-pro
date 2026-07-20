using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
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

    /// <summary>Đăng ký user mới.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "REGISTER_USERNAME_TAKEN"
                ? Conflict(result.ErrorMessage)
                : BadRequest(result.ErrorMessage);
        }

        return CreatedAtAction(nameof(Register), new { id = result.Data!.UserId }, result.Data);
    }

    /// <summary>Đăng nhập, trả về access token + refresh token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        // Ghi đè IP bằng địa chỉ thật — không tin giá trị client gửi lên
        var result = await _sender.Send(command with { ClientIp = ClientIp() }, ct);
        return result.IsSuccess ? Ok(result.Data) : Unauthorized(result.ErrorMessage);
    }

    /// <summary>Đổi refresh token lấy cặp token mới. Token cũ bị thu hồi ngay.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { ClientIp = ClientIp() }, ct);
        return result.IsSuccess ? Ok(result.Data) : Unauthorized(result.ErrorMessage);
    }

    /// <summary>Đăng xuất — thu hồi refresh token. Idempotent, gọi nhiều lần vẫn 204.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        await _sender.Send(command with { ClientIp = ClientIp() }, ct);
        return NoContent();
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
