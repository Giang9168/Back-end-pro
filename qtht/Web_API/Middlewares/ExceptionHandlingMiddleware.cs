using System.Net;
using System.Text.Json;

namespace Web_API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            Application.Common.Exceptions.NotFoundException => (int)HttpStatusCode.NotFound,
            Application.Common.Exceptions.ValidationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        object response = exception switch
        {
            // Kèm chi tiết từng field: { "Password": ["Mật khẩu phải có ít nhất 8 ký tự"] }
            Application.Common.Exceptions.ValidationException ve => new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Dữ liệu không hợp lệ",
                Errors = (IDictionary<string, string[]>?)ve.Errors
            },

            Application.Common.Exceptions.NotFoundException => new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Errors = (IDictionary<string, string[]>?)null
            },

            // Lỗi ngoài dự kiến: KHÔNG trả exception.Message ra ngoài — nó có thể chứa
            // tên bảng, câu SQL, đường dẫn file. Chi tiết đã vào log ở InvokeAsync.
            _ => new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Đã xảy ra lỗi không mong muốn.",
                Errors = (IDictionary<string, string[]>?)null
            }
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }
}
