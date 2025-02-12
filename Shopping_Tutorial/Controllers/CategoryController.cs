using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _context;
        public CategoryController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            CategoryModel category = _context.Categories.Where(x => x.Slug == slug).FirstOrDefault();
            if (category == null) return RedirectToAction("Index");

            IQueryable<ProductModel> productByCategory = _context.Products.Where(x => x.CategoryId == category.Id);

            var count = await productByCategory.CountAsync();
            if (count > 0)
            {
                if (sort_by == "price_increase")
                    productByCategory = productByCategory.OrderBy(x => x.Price);

                else if (sort_by == "price_decrease")
                    productByCategory = productByCategory.OrderByDescending(x => x.Price);

                else if (sort_by == "newest")
                    productByCategory = productByCategory.OrderByDescending(x => x.Id);

                else if (sort_by == "oldest")
                    productByCategory = productByCategory.OrderBy(x => x.Id);

                else if(startprice != "" && endprice != "")
                {
                    double startPriceValue;
                    double endPriceValue;

                    if (double.TryParse(startprice, out startPriceValue) && double.TryParse(endprice, out endPriceValue))
                        productByCategory = productByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    else
                        productByCategory = productByCategory.OrderByDescending(x => x.Id);
                }

                else
                    productByCategory = productByCategory.OrderByDescending(x => x.Id);
            }

            return View(await productByCategory.ToListAsync());
        }
    }
}
