using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityWebApp.Controllers
{
    public class AdminController : BaseController
    {

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager):base(userManager,null,roleManager)
        {
          
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetAllRole()
        {
            return View(_roleManager.Roles.ToList());
        }

        public IActionResult RoleAdd()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RoleAdd(RoleViewModel roleViewModel)
        {
            AppRole role = new();

            role.Name = roleViewModel.Name;

            IdentityResult result = _roleManager.CreateAsync(role).Result;

            if (result.Succeeded)
            {
                return RedirectToAction("GetAllRole");
            }
            else
            {
                AddBaseModelError(result);
            }

            return View(roleViewModel);
        }

        public IActionResult GetAllUser()
        {
            return View(_userManager.Users.ToList());
        }
    }
}
