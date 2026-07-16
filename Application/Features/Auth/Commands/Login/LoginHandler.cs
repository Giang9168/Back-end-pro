using Application.Common.Models;
using Application.DTOs;
using Application.Features.WeatherForecasts.Commands.Create;
using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Login
{
   
    public class LoginHandler
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            // TODO: Lưu vào DbContext khi có EF Core
            var dto = new LoginResponse
            {
                  UserId ="kkk",
                  Username= "kk",
                  Role ="kk",
                  Email="lll",
                  Token ="kks",
                  RefreshToken="lll",
                  ExpiresAt=new DateTime()
             };


            return Result<LoginResponse>.Success(dto);
        }
    }

}
