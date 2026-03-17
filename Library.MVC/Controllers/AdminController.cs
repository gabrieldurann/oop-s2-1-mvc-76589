using Library.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public IActionResult Roles()
    {
        var vm = new RoleManagementViewModel
        {
            Roles = _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(RoleManagementViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Roles = _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
            return View("Roles", vm);
        }

        if (!await _roleManager.RoleExistsAsync(vm.NewRoleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(vm.NewRoleName));
        }

        return RedirectToAction(nameof(Roles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        if (roleName == "Admin")
        {
            return RedirectToAction(nameof(Roles));
        }

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role != null)
        {
            await _roleManager.DeleteAsync(role);
        }

        return RedirectToAction(nameof(Roles));
    }
}