using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace IdentityWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, null, roleManager)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Claims()
        {
            return View(User.Claims.ToList());
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

        public IActionResult RoleUpdate(string id)
        {
            AppRole findRole = _roleManager.FindByIdAsync(id).Result;

            RoleViewModel role = findRole.Adapt<RoleViewModel>();

            return View(role);
        }

        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel roleViewModel)
        {
            AppRole role = _roleManager.FindByIdAsync(roleViewModel.Id).Result;

            if (role != null)
            {
                role.Name = roleViewModel.Name;

                var result = _roleManager.UpdateAsync(role).Result;

                if (result.Succeeded)
                {
                    return RedirectToAction("GetAllRole");
                }
                else
                {
                    AddBaseModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "The update process failed.");
            }

            return View(roleViewModel);
        }

        public IActionResult RoleDelete(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                IdentityResult result = _roleManager.DeleteAsync(role).Result;
            }

            return RedirectToAction("GetAllRole");
        }

        public IActionResult RoleAssign(string id)
        {
            TempData["userId"] = id;

            AppUser user = _userManager.FindByIdAsync(id).Result;

            ViewBag.userName = user.UserName;

            IQueryable<AppRole> roles = _roleManager.Roles;

            IList<string> userRoles = _userManager.GetRolesAsync(user).Result;

            List<RoleAssignViewModel> listRoleAssign = new();

            foreach (var role in roles)
            {
                RoleAssignViewModel roleAssign = new();

                roleAssign.RoleId = role.Id;
                roleAssign.RoleName = role.Name;

                if (userRoles.Contains(role.Name))
                {
                    roleAssign.Exist = true;
                }
                else
                {
                    roleAssign.Exist = false;
                }

                listRoleAssign.Add(roleAssign);
            }

            return View(listRoleAssign);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> listRoleAssignViewModel)
        {
            AppUser user = await _userManager.FindByIdAsync(TempData["userId"].ToString());

            foreach (var item in listRoleAssignViewModel)
            {
                if (item.Exist)
                {
                    await _userManager.AddToRoleAsync(user, item.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return RedirectToAction("GetAllUser");
        }
    }
}
