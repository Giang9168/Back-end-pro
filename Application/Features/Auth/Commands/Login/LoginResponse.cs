using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Login
{
   public record class LoginResponse
    {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
