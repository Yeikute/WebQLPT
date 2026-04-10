using Microsoft.AspNetCore.Mvc;
using WebQLPT.Data;
using System.Threading.Tasks;
using WebQLPT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace WebQLPT.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            // Redirect to unified Auth page (register tab)
            return RedirectToAction("Auth");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // collect simple errors and redirect back to Auth page
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Lỗi: " + string.Join(" | ", errors);
                return RedirectToAction("Auth");
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // always assign KhachThue by default
                await _userManager.AddToRoleAsync(user, "KhachThue");

                // if user requested to be ChuTro, create a request for admin approval
                if (!string.IsNullOrEmpty(model.Role) && model.Role == "ChuTro")
                {
                    var req = new ChuTroRequest
                    {
                        UserId = user.Id,
                        Message = "Yêu cầu trở thành Chủ trọ",
                        Status = "Pending"
                    };
                    _context.ChuTroRequests.Add(req);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đăng ký thành công. Yêu cầu làm Chủ trọ đã được gửi tới Admin.";
                }
                else
                {
                    TempData["Success"] = "Đăng ký thành công. Chào mừng!";
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            if (!result.Succeeded)
            {
                var errs = result.Errors.Select(e => e.Description);
                TempData["Error"] = "Có lỗi khi tạo tài khoản: " + string.Join(" | ", errs);
                return RedirectToAction("Auth");
            }
            
            // should not reach here because success case returns earlier
            return RedirectToAction("Auth");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            // Redirect to unified Auth page (login tab)
            ViewBag.ReturnUrl = returnUrl;
            return RedirectToAction("Auth", new { returnUrl });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Auth(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("Auth", new { returnUrl = model.ReturnUrl });
            }

            // Try find user by email first
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: no user found for email {Email}", model.Email);
                TempData["Error"] = "Tài khoản không tồn tại hoặc email chưa được đăng ký.";
                return RedirectToAction("Auth", new { returnUrl = model.ReturnUrl });
            }

            // Use UserName for sign-in (username was set to email at registration)
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            _logger.LogInformation("Login attempt for {Email}: Succeeded={Succeeded}, LockedOut={LockedOut}, NotAllowed={NotAllowed}", model.Email, result.Succeeded, result.IsLockedOut, result.IsNotAllowed);

            if (result.Succeeded)
            {
                TempData["Success"] = "Đăng nhập thành công.";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                TempData["Error"] = "Tài khoản bị khóa.";
                return RedirectToAction("Auth", new { returnUrl = model.ReturnUrl });
            }

            if (result.IsNotAllowed)
            {
                TempData["Error"] = "Tài khoản chưa được phép đăng nhập.";
                return RedirectToAction("Auth", new { returnUrl = model.ReturnUrl });
            }

            // fallback
            TempData["Error"] = "Email hoặc mật khẩu không đúng.";
            return RedirectToAction("Auth", new { returnUrl = model.ReturnUrl });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public class RegisterViewModel
        {
            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập họ tên")]
            public string FullName { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập email")]
            [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
            public string Password { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
            [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; }

            // Role can be "ChuTro" or "KhachThue". Not required; default is KhachThue.
            public string Role { get; set; }
        }

        public class LoginViewModel
        {
            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập email")]
            [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
            public string ReturnUrl { get; set; }
        }
    }
}
