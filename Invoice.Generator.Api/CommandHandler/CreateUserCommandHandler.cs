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
using Couchbase.Transactions;
using Couchbase.Transactions.Config;
using Couchbase.Transactions.Error;

namespace Invoice.Generator.Api.CommandHandler
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
    {

        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        public CreateUserCommandHandler(IClusterProvider clusterProvider, IBucketProvider bucketProvider)
        {
            _clusterProvider = clusterProvider;
            _bucketProvider = bucketProvider;
        }

        public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var userDetails = new UserDetails();
            userDetails.username = command.Request.Email.ToLower();
            userDetails._type = Constants.DocumentType.User;
            userDetails.iterations = 10000;
            var salt = HelperClass.GenerateSalt();
            userDetails.salt = Convert.ToBase64String(salt);
            userDetails.passwordHash = Convert.ToBase64String(HelperClass.GetPasswordHash(command.Request.Password, salt, userDetails.iterations));
            userDetails.userID = "user::"+Guid.NewGuid();

            return PushToCouchbaseAsync(userDetails, command.Request).Result;
        }

        private async Task<CreateUserResponse> PushToCouchbaseAsync(UserDetails userDetails, CreateUserRequest request)
        {
            var bucket = _bucketProvider.GetBucketAsync("IG_Config_LogIn").Result;
            var collection = bucket.DefaultCollection();
            var cluster = _clusterProvider.GetClusterAsync().Result;


            var newUserID = GetUserId(collection, userDetails.username);
            if (newUserID != null)
                return new CreateUserResponse();
            
            //setting configData for new user
            var userConfigDetails = new UserConfig(userDetails.userID, request.FirstName, request.LastName, Constants.DocumentType.UserConfig);
            
            var response = new CreateUserResponse();

            var transactions = Transactions.Create(cluster, TransactionConfigBuilder.Create().DurabilityLevel(DurabilityLevel.None)
            .Build());

            try
            {
                var result = await transactions.RunAsync(async ctx =>
                {
                    var _userTypeDoc = await ctx.InsertAsync(collection, "" + Guid.NewGuid(), userDetails).ConfigureAwait(false);
                    
                    var _userConfigTypeDoc = await ctx.InsertAsync(collection, "" + Guid.NewGuid(), userConfigDetails).ConfigureAwait(false);
                  
                    await ctx.CommitAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            catch (TransactionFailedException err)
            {
                Console.Error.WriteLine("Transaction Exception: " + err.Message);
                Console.Error.WriteLine("Transaction Exception: " + err.GetBaseException());
                Console.Error.WriteLine("Transaction Exception: " + err.StackTrace);
                foreach (var logLine in err.Result.Logs)
                {
                    Console.Error.WriteLine(logLine);
                }

            }

            response.emailAlreadyExists = false;
            response.isUserCreated = true;
            response.error = null;
            return response;
        }

        private string GetUserId(ICouchbaseCollection collection, string email)
        {
            var cluster = _clusterProvider.GetClusterAsync().Result;

            var n1ql = "SELECT userID FROM IG_Config_LogIn WHERE _type = '_user' AND username = $email";
            var result = cluster.QueryAsync<UserDetails>(n1ql,
                        opt =>
                        {
                            opt.Parameter("$email", email);
                        }).Result;
            var temp = result.Rows.ToListAsync().Result;
            if (temp?.FirstOrDefault()?.userID != null)
                return temp?.FirstOrDefault()?.userID;
            return null;
        }
        

    }
}
