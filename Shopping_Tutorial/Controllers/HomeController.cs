using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _context.Products.Include("Category").Include("Brand").ToList();
            var sliders = _context.Sliders.Where(s => s.Status == 1).ToList();
            ViewBag.Sliders = sliders;
            return View(products);
        }

        public async Task<IActionResult> Wishlist()
        {
            var wishlists = await(from w in _context.Wishlists
                                  join p in _context.Products on w.ProductId equals p.Id 
                                  join u in _context.Users on w.UserId equals u.Id
                                  select new { User = u, Product = p, Wishlist = w }).ToListAsync();
            return View(wishlists);
        }

        public async Task<IActionResult> AddWishlist(int id, WishlistModel wishlist)
        {
            var user = await _userManager.GetUserAsync(User);
            var wishlistModel = new WishlistModel
            {
                ProductId = id,
                UserId = user.Id,
            };
            _context.Wishlists.Add(wishlistModel);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to wishlist successfully!"});
            }
            catch (Exception ex)
            {
            return StatusCode(500, "An error occur while processing the order status");
            }
        }

        public async Task<IActionResult> DeleteWishlistItem(int id)
        {
            var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.Id == id);

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
            TempData["success"] = "Delete wishlist item successfully!";
            return RedirectToAction("Wishlist", "Home");
        }

        public async Task<IActionResult> Compare()
        {
            var compares = await (from c in _context.Compares
                                   join p in _context.Products on c.ProductId equals p.Id
                                   join u in _context.Users on c.UserId equals u.Id
                                   select new { User = u, Product = p, Compare = c }).ToListAsync();
            return View(compares);
        }

        public async Task<IActionResult> AddCompare(int id, CompareModel compare)
        {
            var user = await _userManager.GetUserAsync(User);
            var compareModel = new CompareModel
            {
                ProductId = id,
                UserId = user.Id,
            };
            _context.Compares.Add(compareModel);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to wishlist successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occur while processing the order status");
            }
        }

        public async Task<IActionResult> DeleteCompareItem(int id)
        {
            var compare = await _context.Compares.FirstOrDefaultAsync(w => w.Id == id);

            _context.Compares.Remove(compare);
            await _context.SaveChangesAsync();
            TempData["success"] = "Delete wishlist item successfully!";
            return RedirectToAction("Compare", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
                return View("NotFound");
            else
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
