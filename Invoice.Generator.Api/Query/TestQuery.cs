using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Invoice.Generator.Api.Query
{
    public class TestQuery : IRequest<string>   //IRequest<ReturnType>
    {
        public TestQuery(String message)
        {
            Message = message;
        }

        public string Message;
    }
}
