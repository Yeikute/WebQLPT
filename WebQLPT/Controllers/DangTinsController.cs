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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
        private readonly string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif" };

        public DangTinsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: DangTins
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.DangTins.Include(d => d.ChuTro).Include(d => d.PhongTro).OrderByDescending(d => d.NgayDang);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TieuDe,NoiDung,Gia,PhongTroId,ChuTroId")] DangTin dangTin, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    dangTin.NgayDang = DateTime.Now;

                    // Upload ảnh nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadResult = await UploadImageAsync(imageFile);
                        if (!uploadResult.Success)
                        {
                            ModelState.AddModelError("imageFile", uploadResult.ErrorMessage);
                            LoadSelectLists(dangTin);
                            return View(dangTin);
                        }
                        dangTin.HinhAnh = uploadResult.FilePath;
                    }

                    // Lưu vào database
                    _context.Add(dangTin);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                    LoadSelectLists(dangTin);
                    return View(dangTin);
                }
            }

            LoadSelectLists(dangTin);
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
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "TenChuTro", dangTin.ChuTroId);
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", dangTin.PhongTroId);
            return View(dangTin);
        }

        // POST: DangTins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TieuDe,NoiDung,Gia,HinhAnh,NgayDang,PhongTroId,ChuTroId")] DangTin dangTin, IFormFile? imageFile)
        {
            if (id != dangTin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload ảnh mới nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadResult = await UploadImageAsync(imageFile);
                        if (!uploadResult.Success)
                        {
                            ModelState.AddModelError("imageFile", uploadResult.ErrorMessage);
                            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "TenChuTro", dangTin.ChuTroId);
                            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", dangTin.PhongTroId);
                            return View(dangTin);
                        }

                        // Xóa ảnh cũ
                        if (!string.IsNullOrEmpty(dangTin.HinhAnh))
                        {
                            await DeleteImageAsync(dangTin.HinhAnh);
                        }

                        dangTin.HinhAnh = uploadResult.FilePath;
                    }

                    _context.Update(dangTin);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }
            ViewData["ChuTroId"] = new SelectList(_context.ChuTros, "Id", "TenChuTro", dangTin.ChuTroId);
            ViewData["PhongTroId"] = new SelectList(_context.PhongTros, "Id", "TenPhong", dangTin.PhongTroId);
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
                // Xóa ảnh từ thư mục
                if (!string.IsNullOrEmpty(dangTin.HinhAnh))
                {
                    await DeleteImageAsync(dangTin.HinhAnh);
                }

                _context.DangTins.Remove(dangTin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DangTinExists(int id)
        {
            return _context.DangTins.Any(e => e.Id == id);
        }

        // Helper methods
        private void LoadSelectLists(DangTin dangTin)
        {
            ViewBag.PhongTroId = new SelectList(_context.PhongTros, "Id", "TenPhong", dangTin.PhongTroId);
            ViewBag.ChuTroId = new SelectList(_context.ChuTros, "Id", "TenChuTro", dangTin.ChuTroId);
        }

        private async Task<UploadResult> UploadImageAsync(IFormFile imageFile)
        {
            try
            {
                // Validate WebRootPath
                if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
                {
                    return new UploadResult { Success = false, ErrorMessage = "Không thể tìm thấy thư mục upload" };
                }

                // Validate file size
                if (imageFile.Length > MAX_FILE_SIZE)
                {
                    return new UploadResult { Success = false, ErrorMessage = "Kích thước file quá lớn (tối đa 5MB)" };
                }

                // Validate extension
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!ALLOWED_EXTENSIONS.Contains(extension))
                {
                    return new UploadResult { Success = false, ErrorMessage = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)" };
                }

                // Create upload folder
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return new UploadResult 
                { 
                    Success = true, 
                    FilePath = "/uploads/posts/" + fileName 
                };
            }
            catch (IOException ex)
            {
                return new UploadResult { Success = false, ErrorMessage = $"Lỗi ghi file: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new UploadResult { Success = false, ErrorMessage = $"Lỗi upload: {ex.Message}" };
            }
        }

        private async Task DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return;

                var filePath = Path.Combine(_webHostEnvironment.WebRootPath ?? "", imagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Bỏ qua lỗi xóa file
            }
        }
    }
}

public class UploadResult
{
    public bool Success { get; set; }
    public string FilePath { get; set; }
    public string ErrorMessage { get; set; }
}
