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
    public class DangTinsController : Controller
    {
        private readonly AppDbContext _context;

        public DangTinsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DangTins
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DangTins.Include(d => d.ChuTro).Include(d => d.PhongTro);
            return View(await appDbContext.ToListAsync());
        }

        // GET: DangTins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangTin = await _context.DangTins
                .Include(d => d.ChuTro)
                .Include(d => d.PhongTro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dangTin == null)
            {
                return NotFound();
            }

            return View(dangTin);
        }

        // GET: DangTins/Create
        public IActionResult Create()
        {
            ViewBag.PhongTroId = new SelectList(_context.PhongTros, "Id", "TenPhong");
            ViewBag.ChuTroId = new SelectList(_context.ChuTros, "Id", "TenChuTro");

            return View();
        }

        // POST: DangTins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TieuDe,NoiDung,Gia,HinhAnh,PhongTroId,ChuTroId")] DangTin dangTin)
        {
            if (ModelState.IsValid)
            {
                dangTin.NgayDang = DateTime.Now;

                _context.Add(dangTin);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.PhongTroId = new SelectList(_context.PhongTros, "Id", "TenPhong", dangTin.PhongTroId);
            ViewBag.ChuTroId = new SelectList(_context.ChuTros, "Id", "TenChuTro", dangTin.ChuTroId);
            return View(dangTin);
        }

        // GET: DangTins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangTin = await _context.DangTins.FindAsync(id);
            if (dangTin == null)
            {
                return NotFound();
            }
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "Id", dangTin.ChuTroId);
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "Id", dangTin.PhongTroId);
            return View(dangTin);
        }

        // POST: DangTins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TieuDe,NoiDung,Gia,HinhAnh,NgayDang,PhongTroId,ChuTroId")] DangTin dangTin)
        {
            if (id != dangTin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dangTin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DangTinExists(dangTin.Id))
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
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "Id", dangTin.ChuTroId);
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "Id", dangTin.PhongTroId);
            return View(dangTin);
        }

        // GET: DangTins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangTin = await _context.DangTins
                .Include(d => d.ChuTro)
                .Include(d => d.PhongTro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dangTin == null)
            {
                return NotFound();
            }

            return View(dangTin);
        }

        // POST: DangTins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dangTin = await _context.DangTins.FindAsync(id);
            if (dangTin != null)
            {
                _context.DangTins.Remove(dangTin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DangTinExists(int id)
        {
            return _context.DangTins.Any(e => e.Id == id);
        }
    }
}
