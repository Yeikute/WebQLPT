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
    public class ChuTroesController : Controller
    {
        private readonly AppDbContext _context;

        public ChuTroesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChuTroes
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.ChuTros.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.TenChuTro.Contains(keyword)
                                      || c.SoDienThoai.Contains(keyword)
                                      || c.Email.Contains(keyword));
            }

            return View(await query.ToListAsync());
        }

        // GET: ChuTroes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuTro = await _context.ChuTros
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chuTro == null)
            {
                return NotFound();
            }

            return View(chuTro);
        }

        // GET: ChuTroes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChuTroes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenChuTro,SoDienThoai,Email,DiaChi")] ChuTro chuTro)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return Content("Lỗi: " + string.Join(" | ", errors));
            }

            _context.Add(chuTro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: ChuTroes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuTro = await _context.ChuTros.FindAsync(id);
            if (chuTro == null)
            {
                return NotFound();
            }
            return View(chuTro);
        }

        // POST: ChuTroes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenChuTro,SoDienThoai,Email,DiaChi")] ChuTro chuTro)
        {
            if (id != chuTro.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuTro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuTroExists(chuTro.Id))
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
            return View(chuTro);
        }

        // GET: ChuTroes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuTro = await _context.ChuTros
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chuTro == null)
            {
                return NotFound();
            }

            return View(chuTro);
        }

        // POST: ChuTroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chuTro = await _context.ChuTros.FindAsync(id);
            if (chuTro != null)
            {
                _context.ChuTros.Remove(chuTro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuTroExists(int id)
        {
            return _context.ChuTros.Any(e => e.Id == id);
        }
    }
}
