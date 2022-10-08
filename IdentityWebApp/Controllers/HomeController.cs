using FluentValidation.Results;
using IdentityWebApp.Helpers;
using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using IdentityWebApp.Validations.FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IdentityWebApp.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserViewModelValidator _userViewModelValidator;
        private readonly LoginViewModelValidator _loginViewModelValidator;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, UserViewModelValidator userViewModelValidator, LoginViewModelValidator loginViewModelValidator) : base(userManager, signInManager)
        {
            _logger = logger;
            _userViewModelValidator = userViewModelValidator;
            _loginViewModelValidator = loginViewModelValidator;
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
            ValidationResult results = _userViewModelValidator.Validate(userViewModel);

            if (!results.IsValid)
                return View();

            AppUser appUser = new();

            appUser.UserName = userViewModel.UserName;
            appUser.PhoneNumber = userViewModel.PhoneNumber;
            appUser.Email = userViewModel.Email;

            IdentityResult result = await _userManager.CreateAsync(appUser, userViewModel.Password);

            if (result.Succeeded)
            {
                string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

                string link = Url.Action("ConfirmEmail", "Home", new
                {
                    userId = appUser.Id,
                    token = confirmationToken
                }, protocol: HttpContext.Request.Scheme);

                EmailConfirmation.SendEmail(link, appUser.Email);

                return RedirectToAction(nameof(LogIn));
            }
            else
            {
                AddBaseModelError(result);
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
            ValidationResult results = _loginViewModelValidator.Validate(loginViewModel);
            if (!results.IsValid)
                return View();

            AppUser user = await _userManager.FindByEmailAsync(loginViewModel.Email);

            if (user != null)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Your account has been locked for a while. Please try again later.");
                    return View(loginViewModel);
                }

                if (!_userManager.IsEmailConfirmedAsync(user).Result)
                {
                    ModelState.AddModelError("", "Email is not verified.");
                    return View(loginViewModel);
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

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.status = "Email Confirmed";
            }
            else
            {
                ViewBag.status = "Email not Confirmed";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("Password")] ResetPasswordViewModel resetPasswordViewModel)
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
                    AddBaseModelError(result);
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