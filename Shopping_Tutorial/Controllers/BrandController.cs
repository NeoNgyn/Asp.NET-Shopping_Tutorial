using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _context;
        public BrandController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string slug = "")
        {
            BrandModel brand = _context.Brands.Where(x => x.Slug == slug).FirstOrDefault();
            if (brand == null) return RedirectToAction("Index");

            var productByBrand = _context.Products.Where(x => x.BrandId == brand.Id);
            return View(await productByBrand.OrderByDescending(x => x.Id).ToListAsync());
        }
    }
}
