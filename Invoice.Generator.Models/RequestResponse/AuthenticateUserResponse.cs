using System;

namespace Invoice.Generator.Models.RequestResponse
{
    public class AuthenticateUserResponse
    {
        public bool userExists = false;
        public bool isPasswordValid = false;
        public string error = null;
    }
}
