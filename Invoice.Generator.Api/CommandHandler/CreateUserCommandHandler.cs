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

namespace Invoice.Generator.Api.CommandHandler
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
    {
        public CreateUserCommandHandler()
        {

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

            return PushToCouchbaseAsync(userDetails).Result;
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

        private async Task<CreateUserResponse> PushToCouchbaseAsync(UserDetails userDetails)
        {
            var cluster = await Cluster.ConnectAsync(
                $"{Constants.Couchbase.url2}",
                $"{Constants.Couchbase.username}",
                $"{Constants.Couchbase.password}");

            // get a bucket reference
            var bucket = await cluster.BucketAsync("IG_Config_LogIn");
            // get a user-defined collection reference
            var scope = await bucket.ScopeAsync("_default");
            var collection = await scope.CollectionAsync("_default");

            var response = new CreateUserResponse();
            try
            {
                var result = await collection.InsertAsync("" + Guid.NewGuid(), userDetails);
                //,
                //        options=> { options.Timeout(TimeSpan.FromSeconds(5)); }
                //    );
            }
            catch (Exception ex)
            {
                response.isUserCreated = false;
                response.error = ex.Message;
            }
            response.emailAlreadyExists = false;
            response.isUserCreated = true;
            response.error = null;
            return response;
        }
    }
}
