using System.Reflection;
using Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Chạy trước mọi handler. Thêm behavior khác (logging, transaction...)
            // thì nối vào đây — thứ tự khai báo là thứ tự chạy.
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        // Quét mọi AbstractValidator<T> trong assembly này, y như cách MediatR quét handler
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
