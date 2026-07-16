using Application.Features.WeatherForecasts.Commands.Create;
using Application.Features.WeatherForecasts.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ISender _sender;

    public WeatherForecastController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Lấy danh sách dự báo thời tiết.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetWeatherForecastsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Tạo mới một bản ghi dự báo thời tiết.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}

