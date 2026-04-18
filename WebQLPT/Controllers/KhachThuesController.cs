using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLPT.Data;
using WebQLPT.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebQLPT.Controllers
{
    [Authorize]
    public class KhachThuesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KhachThuesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: KhachThues
        public async Task<IActionResult> Index(string keyword)
        {
            // Build a viewmodel that contains registered users in role KhachThue and manual tenant records
            var vm = new WebQLPT.Models.TenantListViewModel();

            // registered users in role KhachThue
            if (User.IsInRole("Admin"))
            {
                // Admin sees all registered users
                var users = await _userManager.GetUsersInRoleAsync("KhachThue");
                vm.RegisteredUsers = users.ToList();
            }
            else if (User.IsInRole("ChuTro"))
            {
                // ChuTro should see registered users (KhachThue role) so they can gán phòng to them
                var users = await _userManager.GetUsersInRoleAsync("KhachThue");
                vm.RegisteredUsers = users.ToList();
            }
            else if (User.IsInRole("KhachThue"))
            {
                // KhachThue only sees themselves
                var current = await _userManager.GetUserAsync(User);
                if (current != null)
                {
                    var me = await _userManager.FindByIdAsync(current.Id);
                    if (me != null) vm.RegisteredUsers.Add(me);
                    vm.CurrentUserId = current.Id;
                }
            }

            // build RegisteredUserCanAssign mapping
            foreach (var u in vm.RegisteredUsers)
            {
                var isChuTroUser = await _userManager.IsInRoleAsync(u, "ChuTro");
                var hasTenantRecord = await _context.KhachThues.AnyAsync(k => k.UserId == u.Id);
                vm.RegisteredUserCanAssign[u.Id] = !isChuTroUser && !hasTenantRecord;
            }

            // manual tenant records (only Admin/ChuTro see all)
            var tenantsQuery = _context.KhachThues.Include(k => k.PhongTro).Include(k => k.User).AsQueryable();
            if (User.IsInRole("KhachThue"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current != null)
                {
                    tenantsQuery = tenantsQuery.Where(k => k.UserId == current.Id);
                }
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                tenantsQuery = tenantsQuery.Where(k => k.TenKhach.Contains(keyword) || (k.SoDienThoai != null && k.SoDienThoai.Contains(keyword)));
            }
            vm.ManualTenants = await tenantsQuery.ToListAsync();

            return View(vm);
        }

        // GET: KhachThues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachThue = await _context.KhachThues
                .Include(k => k.PhongTro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (khachThue == null)
            {
                return NotFound();
            }

            return View(khachThue);
        }

        // GET: KhachThues/RoomDetails/5
        // View details of the rented room for the tenant
        public async Task<IActionResult> RoomDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachThue = await _context.KhachThues
                .Include(k => k.PhongTro).ThenInclude(p => p.ChuTro)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (khachThue == null)
            {
                return NotFound();
            }

            // Check if current user is the tenant or is an admin/chuTro
            if (User.IsInRole("KhachThue"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current == null || khachThue.UserId != current.Id)
                {
                    return Forbid();
                }
            }

            return View(khachThue);
        }

        // GET: KhachThues/Create
        // Creation should be by assigning a registered user. userId optional: if provided we prefill.
        public async Task<IActionResult> Create(string userId = null)
        {
            var current = await _userManager.GetUserAsync(User);

            // If current is KhachThue and no userId provided, allow creating own tenant record
            if (string.IsNullOrEmpty(userId))
            {
                if (User.IsInRole("KhachThue"))
                {
                    userId = current?.Id;
                }
                else
                {
                    TempData["Error"] = "Vui lòng chọn người dùng để gán phòng (chọn từ danh sách người dùng).";
                    return RedirectToAction(nameof(Index));
                }
            }

            // load user info
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // prevent assigning room if user is already a ChuTro or already has a tenant record
            var isChuTroUser = await _userManager.IsInRoleAsync(user, "ChuTro");
            var hasTenant = await _context.KhachThues.AnyAsync(k => k.UserId == user.Id);
            if (isChuTroUser || hasTenant)
            {
                TempData["Error"] = "Người dùng này không thể được gán phòng (đã là chủ trọ hoặc đã có hồ sơ thuê).";
                return RedirectToAction(nameof(Index));
            }

            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong");
            ViewBag.SelectedUser = user;
            var model = new KhachThue { TenKhach = user.FullName ?? user.UserName, UserId = user.Id };
            return View(model);
        }

        // POST: KhachThues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenKhach,SoDienThoai,CCCD,NgayThue,PhongTroId,UserId")] KhachThue khachThue)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                TempData["Error"] = "Lỗi: " + string.Join(" | ", errors);
                ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", khachThue.PhongTroId);
                return View(khachThue);
            }

            // Không được thuê phòng đã có khách thuê
            var daCoKhach = _context.KhachThues.Any(k => k.PhongTroId == khachThue.PhongTroId);

            if (daCoKhach)
            {
                return Content("Phòng này đã có người thuê!");
            }

            // set owner: if admin/ChuTro and form contains UserId, keep it; otherwise set to current user
            var current = await _userManager.GetUserAsync(User);
            if (!string.IsNullOrEmpty(khachThue.UserId) && (User.IsInRole("Admin") || User.IsInRole("ChuTro")))
            {
                // keep provided UserId
            }
            else if (current != null)
            {
                khachThue.UserId = current.Id;
            }

            // prevent creating duplicate tenant record for a user
            if (!string.IsNullOrEmpty(khachThue.UserId))
            {
                var already = await _context.KhachThues.AnyAsync(k => k.UserId == khachThue.UserId);
                if (already)
                {
                    TempData["Error"] = "Người dùng này đã có hồ sơ thuê.";
                    ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", khachThue.PhongTroId);
                    return View(khachThue);
                }
            }

            _context.Add(khachThue);

            var phong = await _context.PhongTros.FindAsync(khachThue.PhongTroId);

            if (phong != null)
            {
                // Cập nhật trạng thái phòng
                phong.TrangThai = "Đã thuê";
            }


            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm khách thuê thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: KhachThues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var khachThue = await _context.KhachThues.FindAsync(id);
            if (khachThue == null)
            {
                return NotFound();
            }
            // check ownership: KhachThue can only edit their own record
            if (User.IsInRole("KhachThue"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current == null || khachThue.UserId != current.Id)
                {
                    return Forbid();
                }
            }
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "Id", khachThue.PhongTroId);
            return View(khachThue);
        }

        // POST: KhachThues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenKhach,SoDienThoai,CCCD,NgayThue,PhongTroId,UserId")] KhachThue khachThue)
        {
            if (id != khachThue.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // check ownership before updating
                    var existing = await _context.KhachThues.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id);
                    if (existing == null) return NotFound();
                    if (User.IsInRole("KhachThue"))
                    {
                        var current = await _userManager.GetUserAsync(User);
                        if (current == null || existing.UserId != current.Id)
                        {
                            return Forbid();
                        }
                    }

                    _context.Update(khachThue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật khách thuê thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!KhachThueExists(khachThue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                    ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", khachThue.PhongTroId);
                    return View(khachThue);
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = "Lỗi: " + string.Join(" | ", errors);
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", khachThue.PhongTroId);
            return View(khachThue);
        }

        // GET: KhachThues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var khachThue = await _context.KhachThues
                .Include(k => k.PhongTro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (khachThue == null)
            {
                return NotFound();
            }

            // only owner or Admin/ChuTro can delete
            if (User.IsInRole("KhachThue"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current == null || khachThue.UserId != current.Id)
                {
                    return Forbid();
                }
            }

            return View(khachThue);
        }

        // POST: KhachThues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khachThue = await _context.KhachThues.FindAsync(id);
            // check ownership
            if (khachThue != null && User.IsInRole("KhachThue"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current == null || khachThue.UserId != current.Id)
                {
                    return Forbid();
                }
            }
            if (khachThue != null)
            {
                // Lấy phòng của khách
                var phong = await _context.PhongTros.FindAsync(khachThue.PhongTroId);

                // Xóa khách
                _context.KhachThues.Remove(khachThue);
                await _context.SaveChangesAsync();

                // Kiểm tra phòng còn khách không
                var soKhach = await _context.KhachThues
                    .CountAsync(k => k.PhongTroId == phong.Id);

                if (soKhach == 0)
                {
                    phong.TrangThai = "Trống";
                    _context.Update(phong);
                    await _context.SaveChangesAsync();
                }
            }
            TempData["Success"] = "Xóa khách thuê thành công.";
            return RedirectToAction(nameof(Index));
        }

        private bool KhachThueExists(int id)
        {
            return _context.KhachThues.Any(e => e.Id == id);
        }
    }
}
