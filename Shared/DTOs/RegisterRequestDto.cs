using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeBase.Shared.DTOs
{
    public class RegisterRequestDto
    {
        public string NameToShow { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
