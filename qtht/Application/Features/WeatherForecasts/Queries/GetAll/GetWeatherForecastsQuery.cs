using MediatR;

namespace Application.Features.WeatherForecasts.Queries.GetAll;

// Query – không thay đổi state, chỉ đọc dữ liệu
public record GetWeatherForecastsQuery : IRequest<List<WeatherForecastDto>>;

public record WeatherForecastDto
{
    public int Id { get; init; }
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public int TemperatureF { get; init; }
    public string? Summary { get; init; }
}

public class GetWeatherForecastsHandler
    : IRequestHandler<GetWeatherForecastsQuery, List<WeatherForecastDto>>
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<List<WeatherForecastDto>> Handle(
        GetWeatherForecastsQuery request,
        CancellationToken cancellationToken)
    {
        // Dữ liệu giả — endpoint demo CQRS, chưa nối DB
        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecastDto
        {
            Id           = index,
            Date         = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary      = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();

        return Task.FromResult(result);
    }
}
