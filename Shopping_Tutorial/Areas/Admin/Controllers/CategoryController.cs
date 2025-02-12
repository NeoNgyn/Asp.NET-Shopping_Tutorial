using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Publisher, Admin")]
    public class CategoryController : Controller
    {
        private readonly DataContext _context;
        public CategoryController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _context.Categories.ToList();

            const int pageSize = 10;

            if (pg < 1)
                pg = 1;

            int recsCount = category.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "_");
                var slug = await _context.Categories.FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This Category is existed!");
                    return View(category);
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Add category successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Add category failed!";
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
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryModel updatedCategory)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;
            category.Status = updatedCategory.Status;

            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "_");
                var slug = await _context.Categories.FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This Category is existed!");
                    return View(category);
                }

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Updated category successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Updated category failed!";
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
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            _context.Categories.Remove(category);
            _context.SaveChangesAsync();
            TempData["Success"] = "Delete category successfully!";

            return RedirectToAction("Index");
        }
    }
}
