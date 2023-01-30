using Invoice.Generator.Api.Command;
using Invoice.Generator.Models.DB;
using Invoice.Generator.Models.RequestResponse;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Extensions.DependencyInjection;
using System.Linq;
using Couchbase.Transactions.Config;

namespace Invoice.Generator.Api.CommandHandler
{
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResponse>
    {

        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        public AuthenticateUserCommandHandler(IClusterProvider clusterProvider, IBucketProvider bucketProvider)
        {
            _clusterProvider = clusterProvider;
            _bucketProvider = bucketProvider;
        }

        public async Task<AuthenticateUserResponse> Handle(AuthenticateUserCommand command, CancellationToken cancellationToken)
        {
            var bucket = _bucketProvider.GetBucketAsync("IG_Config_LogIn").Result;
            var collection = bucket.DefaultCollection();

            var userDetails = new UserDetails();
            var response = new AuthenticateUserResponse();

            try
            {
                userDetails = GetUserLoginData(collection, command.Request.Email.ToLower());
            }
            catch (Exception e)
            {
                response.error = e.Message;
                return response;
            }

            //user doesn't exist
            if (userDetails == null)
                return new AuthenticateUserResponse();
            
            
            response.userExists = true;

            if (HelperClass.PasswordMatch(command.Request.Password,userDetails.passwordHash, userDetails.salt, userDetails.iterations))
            {
                response.isPasswordValid = true;
            }

            return response;
        }


        private UserDetails GetUserLoginData(ICouchbaseCollection collection, string email)
        {
            var cluster = _clusterProvider.GetClusterAsync().Result;

            var n1ql = "SELECT userID, salt, passwordHash ,iterations FROM IG_Config_LogIn WHERE _type = '_user' AND username = $email";
            var result = cluster.QueryAsync<UserDetails>(n1ql,
                        opt =>
                        {
                            opt.Parameter("$email", email);
                        }).Result;
            var temp = result.Rows.ToListAsync().Result;
            if (temp?.FirstOrDefault() != null)
                return temp?.FirstOrDefault();
            return null;
        }

    }
}
