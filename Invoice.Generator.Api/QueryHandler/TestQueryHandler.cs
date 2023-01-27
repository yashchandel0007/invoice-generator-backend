using Invoice.Generator.Api.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Invoice.Generator.Api.QueryHandler
{
    public class TestQueryHandler: IRequestHandler<TestQuery, string>  //IRequestHandler<query, returnType>
    {
        public TestQueryHandler()
        {

        }

        public async Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(request.Message);
        }
    }
}
