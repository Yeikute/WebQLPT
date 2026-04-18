using Microsoft.AspNetCore.Mvc;
using WebQLPT.Data;
using WebQLPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace WebQLPT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChuTroRequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ChuTroRequestsController(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: ChuTroRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ChuTroRequests.Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.ChuTroRequests.FindAsync(id);
            if (req == null) return NotFound();

            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // ensure role exists then add role ChuTro
            if (!await _roleManager.RoleExistsAsync("ChuTro"))
            {
                await _roleManager.CreateAsync(new IdentityRole("ChuTro"));
            }
            var addRole = await _userManager.AddToRoleAsync(user, "ChuTro");
            if (!addRole.Succeeded)
            {
                TempData["Error"] = "Không thể gán role ChuTro.";
                return RedirectToAction(nameof(Index));
            }

            // create ChuTro record if not exists
            var existing = _context.ChuTros.FirstOrDefault(c => c.Email == user.Email);
            if (existing == null)
            {
                var chu = new ChuTro
                {
                    TenChuTro = user.FullName ?? user.UserName,
                    Email = user.Email,
                    SoDienThoai = "",
                    DiaChi = ""
                };
                _context.ChuTros.Add(chu);
            }

            req.Status = "Approved";
            _context.ChuTroRequests.Update(req);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã phê duyệt yêu cầu và gán role ChuTro.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Assign ChuTro role directly to a user by Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index", "KhachThues");
            }

            if (!await _roleManager.RoleExistsAsync("ChuTro"))
            {
                await _roleManager.CreateAsync(new IdentityRole("ChuTro"));
            }

            var addRole = await _userManager.AddToRoleAsync(user, "ChuTro");
            if (!addRole.Succeeded)
            {
                TempData["Error"] = "Không thể gán role ChuTro.";
                return RedirectToAction("Index", "KhachThues");
            }

            var existing = _context.ChuTros.FirstOrDefault(c => c.Email == user.Email);
            if (existing == null)
            {
                var chu = new ChuTro
                {
                    TenChuTro = user.FullName ?? user.UserName,
                    Email = user.Email,
                    SoDienThoai = user.PhoneNumber ?? string.Empty,
                    DiaChi = string.Empty
                };
                _context.ChuTros.Add(chu);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Đã gán vai trò Chủ trọ cho người dùng.";
            return RedirectToAction("Index", "KhachThues");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.ChuTroRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Rejected";
            _context.ChuTroRequests.Update(req);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yêu cầu đã bị từ chối.";
            return RedirectToAction(nameof(Index));
        }
    }
}
