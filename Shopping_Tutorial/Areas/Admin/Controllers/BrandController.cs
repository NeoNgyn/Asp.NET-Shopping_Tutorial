using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Publisher, Admin")]
    public class BrandController : Controller
    {
        private readonly DataContext _context;

        public BrandController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brands.OrderByDescending(c => c.Id).ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {

            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "_");
                var slug = await _context.Brands.FirstOrDefaultAsync(x => x.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This Brand is existed!");
                    return View(brand);
                }

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Add brand successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Add brand failed!";
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandModel updatedBrand)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);

            brand.Name = updatedBrand.Name;
            brand.Description = updatedBrand.Description;
            brand.Status = updatedBrand.Status;

            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "_");
                var slug = await _context.Brands.FirstOrDefaultAsync(x => x.Slug == brand.Slug);

                _context.Brands.Update(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Updated brand successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Updated brand failed!";
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

        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(x => x.Id == id);

            _context.Brands.Remove(brand);
            _context.SaveChangesAsync();
            TempData["Success"] = "Delete category successfully!";

            return RedirectToAction("Index");
        }
    }
}
