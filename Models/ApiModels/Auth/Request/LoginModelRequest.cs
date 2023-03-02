using System.ComponentModel.DataAnnotations;

namespace Models.ApiModels.Auth.Request;
public class LoginModelRequest
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}