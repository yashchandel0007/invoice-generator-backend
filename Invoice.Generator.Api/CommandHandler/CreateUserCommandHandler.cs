using Invoice.Generator.Api.Command;
using Invoice.Generator.Models.DB;
using Invoice.Generator.Models.RequestResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Invoice.Generator.Models;
using System.Security.Cryptography;
using Couchbase;
using Couchbase.Management.Query;
using Couchbase.KeyValue;
using Couchbase.Extensions.DependencyInjection;
using System.Linq;
using Couchbase.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            userDetails.username = command.Request.Email;
            userDetails._type = Constants.DocumentType.User;
            userDetails.iterations = 10000;
            byte[] salt = GenerateSalt();
            userDetails.salt = Convert.ToBase64String(salt);
            userDetails.passwordHash = Convert.ToBase64String(GetPasswordHash(command.Request.Password, salt, userDetails.iterations));
            userDetails.userID = "user::"+Guid.NewGuid();

            return PushToCouchbaseAsync(userDetails, command.Request).Result;
        }

        private byte[] GetPasswordHash(string password, byte[] salt, int iterations)
        {
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(password, salt, iterations);
            return hash.GetBytes(24);
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[10];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(salt);
            }
            return salt;
        }

        private async Task<CreateUserResponse> PushToCouchbaseAsync(UserDetails userDetails, CreateUserRequest request)
        {
            var bucket = _bucketProvider.GetBucketAsync("IG_Config_LogIn").Result;
            var collection = bucket.DefaultCollection();
            var newUserID = GetUserId(collection, userDetails.username);
            if (newUserID != null)
                return new CreateUserResponse();
            
            //setting configData for new user
            var userConfigDetails = new UserConfig(userDetails.userID, request.FirstName, request.LastName, Constants.DocumentType.UserConfig);
            
            var response = new CreateUserResponse();



            //try
            //{
            //    await transactions.RunAsync(async (ctx) =>
            //    {
            //        //insert log in information
            //        //var _userTypeDoc = await ctx.InsertAsync(collection , "" + Guid.NewGuid(), userDetails).ConfigureAwait(false);
            //        await ctx.QueryAsync<object>(
            //            statement: $"INSERT INTO `IG_Config_LogIn` VALUES ('{"" + Guid.NewGuid()}',{userDetails})",
            //            TransactionQueryConfigBuilder.Create()
            //              );


            //        //insert config data
            //        //var _userConfigTypeDoc = await ctx.InsertAsync(collection, "" + Guid.NewGuid(), userConfigDetails).ConfigureAwait(false);

            //        var _userConfigTypeDoc = await ctx.QueryAsync<object>(
            //            statement: $"INSERT INTO `IG_Config_LogIn` VALUES ('{"" + Guid.NewGuid()}',{userConfigDetails})",
            //            scope: _default);

            //        //await ctx.CommitAsync().ConfigureAwait(false);
            //    });

            //}
            //catch (TransactionCommitAmbiguousException e)
            //{
            //    Console.WriteLine("Transaction possibly committed");
            //    Console.WriteLine(e);
            //}
            //catch (TransactionFailedException e)
            //{
            //    foreach (var logLine in e.Result.Logs)
            //    {
            //        Console.Error.WriteLine(logLine);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response.error = ex.Message;
            //} 

            var options = new ClusterOptions().WithCredentials("Administrator", "Administrator");
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", options).ConfigureAwait(false);
            var bucket1 = await cluster.BucketAsync("IG_Config_LogIn").ConfigureAwait(false);
            var collection1 = await bucket.ScopeAsync("_default").Result.CollectionAsync("_default").ConfigureAwait(false);

            // Create the single Transactions object
            var transactions = Transactions.Create(cluster, TransactionConfigBuilder.Create().DurabilityLevel(DurabilityLevel.None)
        .Build());
            try
            {
                var result = await transactions.RunAsync(async ctx =>
                {
                    var _userTypeDoc = await ctx.InsertAsync(collection1, "" + Guid.NewGuid(), userDetails).ConfigureAwait(false);
                    
                    var _userConfigTypeDoc = await ctx.InsertAsync(collection, "" + Guid.NewGuid(), userConfigDetails).ConfigureAwait(false);
                    
                    Console.WriteLine("inside transaction");
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

        public class Tran
        {
            public string txid;
        }
        

    }
}
