using System;

namespace Invoice.Generator.Models.RequestResponse
{
    public class CreateUserResponse
    {
        public bool isUserCreated =  false;
        public bool emailAlreadyExists = true;
        public string error = null;
    }
}
