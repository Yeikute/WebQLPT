using Microsoft.AspNetCore.Mvc;
using WebQLPT.Data;
using WebQLPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace WebQLPT.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int? dangTinId)
        {
            ViewBag.DangTinId = dangTinId;
            if (dangTinId != null)
            {
                var dt = await _context.DangTins.FindAsync(dangTinId);
                ViewBag.DangTinTitle = dt?.TieuDe;
            }

            var model = new PaymentInputModel { DangTinId = dangTinId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutPost(PaymentInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                if (model.DangTinId != null)
                {
                    var dt = await _context.DangTins.FindAsync(model.DangTinId);
                    ViewBag.DangTinTitle = dt?.TieuDe;
                }
                return View("Checkout", model);
            }

            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Số tiền phải lớn hơn 0.");
                if (model.DangTinId != null)
                {
                    var dt = await _context.DangTins.FindAsync(model.DangTinId);
                    ViewBag.DangTinTitle = dt?.TieuDe;
                }
                return View("Checkout", model);
            }

            var payment = new Payment
            {
                UserId = user.Id,
                DangTinId = model.DangTinId,
                Amount = model.Amount,
                Status = "Pending"
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Mock payment succeeded
            payment.Status = "Succeeded";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thanh toán thành công.";
            return RedirectToAction("Success", new { id = payment.Id });
        }

        public async Task<IActionResult> Success(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        public class PaymentInputModel
        {
            public int? DangTinId { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập số tiền")]
            [System.ComponentModel.DataAnnotations.Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
            public decimal Amount { get; set; }
        }
    }
}
