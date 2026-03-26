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
    public class PhongTroesController : Controller
    {
        private readonly AppDbContext _context;

        public PhongTroesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PhongTroes
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.PhongTros.AsQueryable();

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

            return View(phongTro);
        }

        // GET: PhongTroes/Create
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
        public async Task<IActionResult> Create([Bind("Id,TenPhong,Gia,TrangThai,MoTa,ChuTroId")] PhongTro phongTro)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Content("Lỗi: " + string.Join(" | ", errors));
            }

            _context.Add(phongTro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: PhongTroes/Edit/5
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhongTroExists(phongTro.Id))
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
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "Id", phongTro.ChuTroId);
            return View(phongTro);
        }

        // GET: PhongTroes/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phongTro = await _context.PhongTros.FindAsync(id);
            if (phongTro != null)
            {
                _context.PhongTros.Remove(phongTro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhongTroExists(int id)
        {
            return _context.PhongTros.Any(e => e.Id == id);
        }
    }
}
