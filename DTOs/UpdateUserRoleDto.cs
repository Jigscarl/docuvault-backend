using System.ComponentModel.DataAnnotations;
namespace Docuvault.Api.DTOs;

public class UpdateUserRoleDto
{
    [Required]
    [RegularExpression(
        "^(User|Admin)$", ErrorMessage ="Role must be either User or Admin"
    )]
    public string Role {get; set;} = "User";
}