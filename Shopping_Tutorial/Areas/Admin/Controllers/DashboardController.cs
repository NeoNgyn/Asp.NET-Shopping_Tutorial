using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            var countProduct = _context.Products.Count();
            var orderCount = _context.Orders.Count();
            var categortCount = _context.Categories.Count();
            var userCount = _context.Users.Count();
            ViewBag.ProductCount = countProduct;
            ViewBag.OrderCount = orderCount;
            ViewBag.UserCount = userCount;
            ViewBag.CategoryCount = categortCount;
            return View();
        }

        [HttpPost]
        [Route("GetChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var data = await _context.Statics.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            })
                .ToListAsync();

            return Json(data);
        }

        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data = _context.Statics
                .Where(s => s.DateCreated >= startDate && s.DateCreated <= endDate)
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }

        [HttpPost]
        [Route("FilterData")]
        public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Statics.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(q => q.DateCreated >= fromDate);

            if (toDate.HasValue)
                query = query.Where(q => q.DateCreated <= toDate);

            var data = query
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }
    }
}
