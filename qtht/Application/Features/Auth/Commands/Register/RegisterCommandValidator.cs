using System.Text;
using FluentValidation;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    // BCrypt chỉ xét 72 byte đầu, phần dư bị cắt âm thầm — chặn sớm để người dùng
    // không tưởng mật khẩu dài là an toàn hơn thực tế.
    private const int MaxPasswordBytes = 72;

    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống")
            .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự")
            .MaximumLength(100).WithMessage("Tên đăng nhập tối đa 100 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự")
            .Must(p => p is null || Encoding.UTF8.GetByteCount(p) <= MaxPasswordBytes)
                .WithMessage($"Mật khẩu quá dài (tối đa {MaxPasswordBytes} byte)");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
