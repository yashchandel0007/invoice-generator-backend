using System;

namespace Invoice.Generator.Models.RequestResponse
{
    public class CreateUserResponse
    {
        public bool isUserCreated =  true;
        public bool emailAlreadyExists = false;
        public string error = null;
    }
}
