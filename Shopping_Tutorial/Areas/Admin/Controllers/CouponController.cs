using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Coupon")]
    [Authorize(Roles = "Publisher, Admin")]
    public class CouponController : Controller
    {
        private readonly DataContext _context;
        
        public CouponController(DataContext context)
        {
            _context = context;
            
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons.ToListAsync();
            ViewBag.Coupons = coupons;
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                TempData["success"] = "Create coupon successfully!";
                return RedirectToAction("Index", "Coupon");
            }
            else
            {
                TempData["Error"] = "Create coupon failed!";
                List<string> errorList = new List<string>();
                foreach (var item in ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        errorList.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errorList);
                return BadRequest(errorMessage);
            }
        }

        
    }
}
