using IdentityWebApp.Enums;
using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityWebApp.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly ILogger<MemberController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public MemberController(ILogger<MemberController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.gender = new SelectList(Enum.GetNames(typeof(GenderEnum)));
            
            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model)
        {
            ModelState.Remove("Password");
            if (!ModelState.IsValid)
                return View(model);

            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                await _signInManager.SignOutAsync();

                await _signInManager.SignInAsync(user, true);

                ViewBag.success = true;
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            return View(model);
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordChange(UserPasswordChangeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;

            if (user != null)
            {
                bool passwordExist = _userManager.CheckPasswordAsync(user, model.PasswordOld).Result;

                if (passwordExist)
                {
                    IdentityResult result = _userManager.ChangePasswordAsync(user, model.PasswordOld, model.PasswordNew).Result;

                    if (result.Succeeded)
                    {
                        _userManager.UpdateSecurityStampAsync(user);

                        _signInManager.SignOutAsync();

                        _signInManager.PasswordSignInAsync(user, model.PasswordNew, true, false);

                        ViewBag.success = true;
                        return View();
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("PasswordOld", "Your old password is incorrect");
                }
            }
            return View(model);
        }

        public void Logout()
        {
            _signInManager.SignOutAsync();
        }
    }
}
