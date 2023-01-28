using Invoice.Generator.Models.RequestResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Invoice.Generator.Api.Command
{
    public class CreateUserCommand : IRequest<CreateUserResponse>
    {
        public CreateUserCommand(CreateUserRequest request)
        {
            Request = request;
        }

        public CreateUserRequest Request;
    }
}
