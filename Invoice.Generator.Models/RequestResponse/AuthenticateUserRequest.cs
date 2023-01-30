using System;
using System.ComponentModel.DataAnnotations;

namespace Invoice.Generator.Models.RequestResponse
{
    public class AuthenticateUserRequest
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
