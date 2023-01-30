using Invoice.Generator.Models.RequestResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Invoice.Generator.Api.Command
{
    public class AuthenticateUserCommand : IRequest<AuthenticateUserResponse>
    {
        public AuthenticateUserCommand(AuthenticateUserRequest request)
        {
            Request = request;
        }

        public AuthenticateUserRequest Request;
    }
}
