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
    public class KhachThuesController : Controller
    {
        private readonly AppDbContext _context;

        public KhachThuesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: KhachThues
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.KhachThues.Include(k => k.PhongTro).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(k => k.TenKhach.Contains(keyword));
            }

            return View(await query.ToListAsync());
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

        // GET: KhachThues/Create
        public IActionResult Create()
        {
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong");
            return View();
        }

        // POST: KhachThues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenKhach,SoDienThoai,CCCD,NgayThue,PhongTroId")] KhachThue khachThue)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Content("Lỗi: " + string.Join(" | ", errors));
            }

            // Không được thuê phòng đã có khách thuê
            var daCoKhach = _context.KhachThues.Any(k => k.PhongTroId == khachThue.PhongTroId);

            if (daCoKhach)
            {
                return Content("Phòng này đã có người thuê!");
            }

            _context.Add(khachThue);

            var phong = await _context.PhongTros.FindAsync(khachThue.PhongTroId);

            if (phong != null)
            {
                // Cập nhật trạng thái phòng
                phong.TrangThai = "Đã thuê";
            }


            await _context.SaveChangesAsync();
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
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "Id", khachThue.PhongTroId);
            return View(khachThue);
        }

        // POST: KhachThues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenKhach,SoDienThoai,CCCD,NgayThue,PhongTroId")] KhachThue khachThue)
        {
            if (id != khachThue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khachThue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachThueExists(khachThue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "Id", khachThue.PhongTroId);
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

            return View(khachThue);
        }

        // POST: KhachThues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khachThue = await _context.KhachThues.FindAsync(id);

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

            return RedirectToAction(nameof(Index));
        }

        private bool KhachThueExists(int id)
        {
            return _context.KhachThues.Any(e => e.Id == id);
        }
    }
}
