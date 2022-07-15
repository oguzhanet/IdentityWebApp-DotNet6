﻿using IdentityWebApp.Helpers;
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
                    ModelState.AddModelError("", "Hasabınız bir süreliğine kitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
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
                        ModelState.AddModelError("", $"Hasabınız {failCount} başarısız girişten dolayı 20 dakika süreyle kitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"{failCount} kez başarısız giriş yapıldı.");
                        ModelState.AddModelError("", "Email veya Şifresiniz yanlış.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
            }

            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
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

                ResetPasswordHelper.ResetPasswordSendEmail(passwordResetLink);

                ViewBag.status = "successfull";
            }
            else
            {
                ModelState.AddModelError("", "Email adresi bulunamadı.");
            }

            return View(resetPasswordViewModel);
        }
    }
}