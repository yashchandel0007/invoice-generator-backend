using System;
using System.ComponentModel.DataAnnotations;

namespace Invoice.Generator.Models.RequestResponse
{
    public class CreateUserRequest
    {
        [Required]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }
        [DataType(DataType.Text)]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
