using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _context;
        public ProductController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _context.Products.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)).ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == null) return RedirectToAction("Index");
            var productById = _context.Products.Include(p => p.Ratings).Where(x => x.Id == id).FirstOrDefault();

            var relatedProducts = await _context.Products.Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id).Take(4).ToListAsync();   

            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailViewModel
            {
                ProductDetails = productById,
                
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star
                };

                _context.Ratings.Add(ratingEntity);
                await _context.SaveChangesAsync();

                TempData["success"] = "Rating product successfully!";

                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["Error"] = "Update product failed!";
                List<string> errorList = new List<string>();
                foreach (var item in ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        errorList.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errorList);
                return RedirectToAction("Detail", new { id = rating.ProductId });

            }
        }
    }
}
