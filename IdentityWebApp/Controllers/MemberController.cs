using IdentityWebApp.Models;
using IdentityWebApp.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    }
}
