using FluentValidation;

namespace Application.Features.Auth.Commands.ExternalLogin;

public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Thiếu thông tin nhà cung cấp đăng nhập");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Thiếu token đăng nhập");
    }
}
