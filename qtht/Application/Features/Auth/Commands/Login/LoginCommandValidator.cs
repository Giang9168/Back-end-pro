using FluentValidation;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // Chỉ kiểm tra "có nhập gì chưa". KHÔNG áp quy tắc độ dài/độ mạnh ở đây:
        // quy tắc mật khẩu có thể đổi theo thời gian, mà tài khoản cũ vẫn phải
        // đăng nhập được. Sai hay đúng để LoginHandler đối chiếu hash quyết định.
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Vui lòng nhập tên đăng nhập hoặc email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu");
    }
}
