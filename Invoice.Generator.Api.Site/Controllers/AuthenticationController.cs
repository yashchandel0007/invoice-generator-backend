using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Invoice.Generator.Api.Query;
using Invoice.Generator.Models.RequestResponse;
using Invoice.Generator.Api.Command;
using Couchbase.Extensions.DependencyInjection;
using Couchbase;

namespace Invoice.Generator.Api.Site.Controllers
{
    [Route("authentication")]
    public class AuthenticationController : Controller
    {

        private readonly IMediator _mediator;
        public AuthenticationController(IMediator mediator) 
        { 
            _mediator = mediator;
        }


        [Route("testapicall")]
        [HttpGet]
        public async Task<IActionResult> TestApiCallAsync(string message)
        {
            var result = await _mediator.Send(new TestQuery(message));
            return Ok(result);
        }

        [Route("user/create")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody]CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request!");
            }
            var result = await _mediator.Send(new CreateUserCommand(request));
            return Ok(result);
        }

        [Route("user/authenticate")]
        [HttpPost]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request!");
            }
            var result = await _mediator.Send(new AuthenticateUserCommand(request));
            return Ok(result);
        }

    }
}
