using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLPT.Data;
using WebQLPT.Models;

namespace WebQLPT.Controllers
{
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

    public class PhongTroesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhongTroesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: PhongTroes
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.PhongTros.AsQueryable();

            // KhachThue chỉ được xem phòng mà họ đã thuê
            if (User.IsInRole("KhachThue"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var rentedRoomIds = _context.KhachThues
                        .Where(k => k.UserId == user.Id)
                        .Select(k => k.PhongTroId)
                        .ToList();

                    query = query.Where(p => rentedRoomIds.Contains(p.Id));
                }
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.TenPhong.Contains(keyword));
            }

            return View(await query.ToListAsync());
        }

        // GET: PhongTroes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phongTro = await _context.PhongTros
                .Include(p => p.ChuTro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phongTro == null)
            {
                return NotFound();
            }

            // KhachThue chỉ được xem chi tiết phòng mà họ đã thuê
            if (User.IsInRole("KhachThue"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var isRented = await _context.KhachThues.AnyAsync(k => k.UserId == user.Id && k.PhongTroId == id);
                    if (!isRented)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Challenge();
                }
            }

            return View(phongTro);
        }

        // GET: PhongTroes/Rent/5
        [HttpGet]
        [Authorize(Roles = "KhachThue,Admin,ChuTro")]
        public async Task<IActionResult> Rent(int? id)
        {
            if (id == null) return NotFound();
            var phong = await _context.PhongTros.Include(p => p.ChuTro).FirstOrDefaultAsync(p => p.Id == id);
            if (phong == null) return NotFound();

            if (phong.TrangThai != "Trống")
            {
                TempData["Error"] = "Phòng hiện không sẵn sàng để thuê.";
                return RedirectToAction(nameof(Index));
            }

            return View(phong);
        }

        // POST: PhongTroes/Rent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "KhachThue,Admin,ChuTro")]
        public async Task<IActionResult> RentConfirmed(int id)
        {
            var phong = await _context.PhongTros.FindAsync(id);
            if (phong == null) return NotFound();

            if (phong.TrangThai != "Trống")
            {
                TempData["Error"] = "Phòng đã được thuê.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // create KhachThue record for this user
            var kh = new KhachThue
            {
                TenKhach = user.FullName ?? user.UserName,
                SoDienThoai = user.PhoneNumber ?? "",
                CCCD = string.Empty,
                NgayThue = DateTime.Now,
                PhongTroId = phong.Id,
                UserId = user.Id
            };

            // prevent double rent by checking again
            var daCoKhach = _context.KhachThues.Any(k => k.PhongTroId == phong.Id);
            if (daCoKhach)
            {
                TempData["Error"] = "Phòng này đã có người thuê.";
                return RedirectToAction(nameof(Index));
            }

            _context.KhachThues.Add(kh);
            phong.TrangThai = "Đã thuê";
            _context.PhongTros.Update(phong);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bạn đã thuê phòng thành công.";
            return RedirectToAction("Index", "KhachThues");
        }

        // GET: PhongTroes/Create
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ChuTro,Admin")]
        public IActionResult Create()
        {
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "Id");
            return View();
        }

        // POST: PhongTroes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ChuTro,Admin")]
        public async Task<IActionResult> Create([Bind("Id,TenPhong,Gia,TrangThai,MoTa,ChuTroId")] PhongTro phongTro)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                TempData["Error"] = "Lỗi: " + string.Join(" | ", errors);
                ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "Id", phongTro.ChuTroId);
                return View(phongTro);
            }
            _context.Add(phongTro);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm phòng trọ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: PhongTroes/Edit/5
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ChuTro,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phongTro = await _context.PhongTros.FindAsync(id);
            if (phongTro == null)
            {
                return NotFound();
            }
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "TenChuTro", phongTro.ChuTroId);
            return View(phongTro);
        }

        // POST: PhongTroes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ChuTro,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenPhong,Gia,TrangThai,MoTa,ChuTroId")] PhongTro phongTro)
        {
            if (id != phongTro.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phongTro);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật phòng trọ thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!PhongTroExists(phongTro.Id))
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
                    ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "TenChuTro", phongTro.ChuTroId);
                    return View(phongTro);
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["Error"] = "Lỗi: " + string.Join(" | ", errors);
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "TenChuTro", phongTro.ChuTroId);
            return View(phongTro);
        }

        // GET: PhongTroes/Delete/5
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ChuTro,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phongTro = await _context.PhongTros
                .Include(p => p.ChuTro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (phongTro == null)
            {
                return NotFound();
            }

            return View(phongTro);
        }

        // POST: PhongTroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ChuTro,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phongTro = await _context.PhongTros.FindAsync(id);
            if (phongTro != null)
            {
                _context.PhongTros.Remove(phongTro);
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa phòng trọ thành công.";
            return RedirectToAction(nameof(Index));
        }

        private bool PhongTroExists(int id)
        {
            return _context.PhongTros.Any(e => e.Id == id);
        }
    }
}
