using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using WebQLPT.Models;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Linq;

namespace WebQLPT.Controllers
{
    // Development-only helper to ensure an admin user exists
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;

        public SetupController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config, IHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> EnsureAdmin()
        {
            // only allow in Development environment for safety
            if (!_env.IsDevelopment())
            {
                return Forbid();
            }

            var adminEmail = _config["AdminUser:Email"] ?? "admin@example.com";
            var adminPassword = _config["AdminUser:Password"] ?? "Admin@123";
            var adminFullName = _config["AdminUser:FullName"] ?? "Administrator";

            // ensure roles
            var roles = new[] { "Admin", "ChuTro", "KhachThue" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var admin = await _userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = adminFullName };
                var create = await _userManager.CreateAsync(admin, adminPassword);
                if (!create.Succeeded)
                {
                    return BadRequest(new { success = false, errors = create.Errors.Select(e => e.Description) });
                }
            }

            if (!await _userManager.IsInRoleAsync(admin, "Admin"))
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
            }

            if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                await _userManager.UpdateAsync(admin);
            }

            if (admin.LockoutEnabled)
            {
                admin.LockoutEnabled = false;
                await _userManager.UpdateAsync(admin);
            }

            return Ok(new { success = true, email = adminEmail, password = adminPassword });
        }
    }
}
