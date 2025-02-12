using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.OrderByDescending(x => x.Id).Include(x => x.Category).Include(x => x.Brand).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == null) return RedirectToAction("Index");
            var productById = _context.Products.Where(x => x.Id == id).FirstOrDefault();

            return View(productById);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "_");
                var slug = await _context.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This product is existed!");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Add product successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Add product failed!";
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
            var product = await  _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductModel updatedProduct)
        {
            var product = await _context.Products.FirstOrDefaultAsync(y => y.Id == id);

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {

                product.Name = updatedProduct.Name;
                product.Description = updatedProduct.Description;
                product.Price = updatedProduct.Price;
                product.CapitalPrice = updatedProduct.CapitalPrice;
                product.CategoryId = updatedProduct.CategoryId;
                product.BrandId = updatedProduct.BrandId;
                product.Slug = product.Name.Replace(" ", "_");


                if (product.ImageUpload != updatedProduct.ImageUpload)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }

                if(product.ImageUpload == null)
                    product.ImageUpload = updatedProduct.ImageUpload;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Update product successfully!";
                return RedirectToAction("Index");
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
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            string oldImageName = Path.Combine(uploadDir, product.Image);

            if (System.IO.File.Exists(oldImageName))
                System.IO.File.Delete(oldImageName);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Delete product successfully!";

            return RedirectToAction("Index");
        }

        //Add more quantity to product
        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int id)
        {
            var productQuantity = await _context.ProductQuantities.Where(p => p.ProductId == id).ToListAsync();
            ViewBag.ProductByQuantity = productQuantity;
            ViewBag.ProductId = id;
            return View();
        }

        [Route("UpdateProductQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProductQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = await _context.Products.FindAsync(productQuantityModel.ProductId);

            if (product == null) 
                return NotFound();

            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.CreatedDate = DateTime.Now;

            _context.ProductQuantities.Add(productQuantityModel);
            await _context.SaveChangesAsync();
            TempData["success"] = "Update product quantity successfully!";

            return RedirectToAction("AddQuantity", "Product",  new { Id = productQuantityModel.ProductId });
        }
    }
}
