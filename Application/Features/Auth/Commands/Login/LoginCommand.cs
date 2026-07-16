using Application.Common.Models;
using Application.DTOs;
using Application.Features.Auth.Commands.Login;
using MediatR;

namespace Application.Features.WeatherForecasts.Commands.Create;

// Command – thay đổi state (thêm mới dữ liệu)
public record LoginCommand(
    DateOnly Date,
    int      TemperatureC,
    string?  Summary
) : IRequest<Result<LoginResponse>>

{
    public required string EmailOrUsername { get; set; }
    public required string Password { get; set; }

}
