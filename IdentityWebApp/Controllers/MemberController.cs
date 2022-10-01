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
    public class MemberController : BaseController
    {
        private readonly ILogger<MemberController> _logger;
       

        public MemberController(ILogger<MemberController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(userManager, signInManager)  
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            AppUser user = CurrentUser;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.gender = new SelectList(Enum.GetNames(typeof(GenderEnum)));

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model, IFormFile userPicture)
        {
            //ModelState.Remove("Password");
            //if (!ModelState.IsValid)
            //    return View(model);
            
            ViewBag.gender = new SelectList(Enum.GetNames(typeof(GenderEnum)));

            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (userPicture != null && userPicture.Length > 0)
            {
                string[] validImageFileTypes = { ".JPG", ".JPEG", ".PNG", ".TIF", ".TIFF", ".GIF", ".BMP", ".ICO" };
                bool isValidFileExtension = validImageFileTypes.Any(t => t == Path.GetExtension(userPicture.FileName).ToUpper());
                
                if (isValidFileExtension)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);

                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);

                        user.Picture = "/UserPicture/" + fileName;
                    }
                }
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.City = model.City;
            user.BirthDate = model.BirthDate;
            user.Gender = (int)model.Gender;

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
                AddBaseModelError(result);
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

            AppUser user = CurrentUser;

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
                        AddBaseModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("PasswordOld", "Your old password is incorrect");
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult EditorPage()
        {
            return View();
        }

        [Authorize(Roles = "Manager,Admin")]
        public IActionResult ManagerPage()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public void Logout()
        {
            _signInManager.SignOutAsync();
        }

        [Authorize(Policy = "IstanbulPolicy")]
        public IActionResult IstanbulPage()
        {
            return View();
        }
    }
}
