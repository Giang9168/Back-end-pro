using FluentValidation;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.Common.Behaviours;

/// <summary>
/// Chặn giữa ISender.Send() và handler: tìm mọi validator khai cho request,
/// chạy hết, gom lỗi lại rồi ném ValidationException.
///
///     _sender.Send(command)
///        ▼
///     ValidationBehaviour   ← ở đây
///        ▼
///     Handler               ← chỉ chạy khi dữ liệu đã sạch
///
/// Nhờ vậy handler không cần kiểm tra hình thức, và mọi command đều được
/// kiểm tra dù đến từ HTTP, job nền hay test.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Request không có validator thì đi thẳng, không tốn gì
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Gom lỗi theo tên field: { "Password": ["quá ngắn", "thiếu chữ số"] }
        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        if (failures.Count != 0)
        {
            // ExceptionHandlingMiddleware bắt cái này và trả 400
            throw new ValidationException(failures);
        }

        return await next();
    }
}
