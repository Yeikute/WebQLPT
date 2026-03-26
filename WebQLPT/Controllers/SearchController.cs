using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLPT.Data;

namespace WebQLPT.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return View();
            }

            keyword = keyword.ToLower();

            //Phòng trọ
            var phongTro = await _context.PhongTros
            .Include(p => p.KhachThues)
            .Where(p => p.TenPhong.ToLower().Contains(keyword)
            || p.KhachThues.Any(k => k.TenKhach.ToLower().Contains(keyword)))
            .ToListAsync();

            //Khách thuê
            var khachThue = await _context.KhachThues
            .Include(k => k.PhongTro)
            .ThenInclude(p => p.ChuTro)
            .Where(k => k.TenKhach.ToLower().Contains(keyword))
            .ToListAsync();

            //Chủ trọ
            var chuTro = await _context.ChuTros
                .Include(c => c.PhongTros)
                .ThenInclude(p => p.KhachThues)
                .Where(c => c.TenChuTro.ToLower().Contains(keyword)
                || c.PhongTros.Any(p => p.TenPhong.ToLower().Contains(keyword))
                || c.PhongTros.Any(p => p.KhachThues.Any(k => k.TenKhach.ToLower().Contains(keyword))))
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.PhongTro = phongTro;
            ViewBag.KhachThue = khachThue;
            ViewBag.ChuTro = chuTro;

            return View();
        }
    }
}