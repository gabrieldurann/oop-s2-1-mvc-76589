using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models.ViewModels;

public class RoleManagementViewModel
{
    public List<string> Roles { get; set; } = new();

    [Required]
    public string NewRoleName { get; set; } = string.Empty;
}