using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
