using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebQLPT.Data;
using WebQLPT.Models;

namespace WebQLPT.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.SoPhong = _context.PhongTros.Count();
            ViewBag.SoKhach = _context.KhachThues.Count();

            //ViewBag.PhongTrong = _context.PhongTros.Count(p => p.TrangThai == "Trống");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
