using FluentValidation.Results;
using IdentityWebApp.Helpers;
using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using IdentityWebApp.Validations.FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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

        public IActionResult FacebookLogin(string ReturnUrl)
        {
            string redirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);

            return new ChallengeResult("Facebook", properties);
        }

        public IActionResult GoogleLogin(string ReturnUrl)
        {
            string redirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return new ChallengeResult("Google", properties);
        }

        public IActionResult MicrosoftLogin(string ReturnUrl)
        {
            string redirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);

            return new ChallengeResult("Microsoft", properties);
        }

        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            ExternalLoginInfo loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, true);

                if (result.Succeeded)
                {
                    return RedirectToAction(ReturnUrl);
                }
                else
                {
                    AppUser user = new();

                    user.Email = loginInfo.Principal.FindFirst(ClaimTypes.Email).Value;
                    string externalUserId = loginInfo.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (loginInfo.Principal.HasClaim(x=>x.Type==ClaimTypes.Name))
                    {
                        string userName = loginInfo.Principal.FindFirst(ClaimTypes.Name).Value;

                        userName = userName.Replace(' ', '_').ToLower() + externalUserId.Substring(0, 5).ToString();
                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = loginInfo.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    AppUser findUser = await _userManager.FindByEmailAsync(user.Email);

                    if (findUser==null)
                    {
                        IdentityResult createResult = await _userManager.CreateAsync(user);

                        if (createResult.Succeeded)
                        {
                            IdentityResult loginResult = await _userManager.AddLoginAsync(user, loginInfo);

                            if (loginResult.Succeeded)
                            {
                                //await _signInManager.SignInAsync(user, true);
                                await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, true);
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                AddBaseModelError(loginResult);
                            }
                        }
                        else
                        {
                            AddBaseModelError(createResult);
                        }
                    }
                    else
                    {
                        IdentityResult loginResult = await _userManager.AddLoginAsync(findUser, loginInfo);

                        if (loginResult.Succeeded)
                        {
                            await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, true);
                            return Redirect(ReturnUrl);
                        }
                    }
                }

                List<string> errors = ModelState.Values.SelectMany(x => x.Errors).Select(z => z.ErrorMessage).ToList();

                return View("ErrorPage",errors);
            }
        }

        public IActionResult ErrorPage()
        {
            return View();
        }
    }
}