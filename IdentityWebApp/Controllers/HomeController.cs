using IdentityWebApp.Helpers;
using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IdentityWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Member");
                
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            AppUser appUser = new();

            appUser.UserName = userViewModel.UserName;
            appUser.PhoneNumber = userViewModel.PhoneNumber;
            appUser.Email = userViewModel.Email;

            IdentityResult result = await _userManager.CreateAsync(appUser, userViewModel.Password);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(LogIn));
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }

            return View(userViewModel);
        }

        public IActionResult LogIn(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
                return View();
            
            AppUser user = await _userManager.FindByEmailAsync(loginViewModel.Email);

            if (user != null)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Your account has been locked for a while. Please try again later.");
                    return View();
                }

                await _signInManager.SignOutAsync();

                Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, false);

                if (signInResult.Succeeded)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);

                    if (TempData["ReturnUrl"] != null)
                    {
                        return Redirect(TempData["ReturnUrl"].ToString());
                    }

                    return RedirectToAction(nameof(Index), "Member");
                }
                else
                {
                    await _userManager.AccessFailedAsync(user);

                    int failCount = await _userManager.GetAccessFailedCountAsync(user);

                    if (failCount >= 3)
                    {
                        await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(20)));
                        ModelState.AddModelError("", $"Your account has been locked for 20 minutes due to {failCount} failed logins. Please try again later.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Failed login {failCount} times.");
                        ModelState.AddModelError("", "Your Email or Password is incorrect.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found.");
            }

            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword([Bind("Email")] ResetPasswordViewModel resetPasswordViewModel)
        {
            AppUser user = _userManager.FindByEmailAsync(resetPasswordViewModel.Email).Result;

            if (user != null)
            {
                string passwordResetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;

                string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                }, HttpContext.Request.Scheme);

                ResetPasswordHelper.ResetPasswordSendEmail(passwordResetLink, resetPasswordViewModel.Email);

                ViewBag.status = "success";
            }
            else
            {
                ModelState.AddModelError("", "Email not found.");
            }

            return View(resetPasswordViewModel);
        }

        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
            //return RedirectToAction(nameof(LogIn));
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("Password")]ResetPasswordViewModel resetPasswordViewModel)
        {
            string userId = TempData["userId"].ToString();
            string token = TempData["token"].ToString();

            AppUser user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                IdentityResult? result = await _userManager.ResetPasswordAsync(user, token, resetPasswordViewModel.Password);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);

                    ViewBag.status = "success";
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
                ModelState.AddModelError("", "Could not reset password. Please try again later.");
            }

            return View(resetPasswordViewModel);
        }
    }
}