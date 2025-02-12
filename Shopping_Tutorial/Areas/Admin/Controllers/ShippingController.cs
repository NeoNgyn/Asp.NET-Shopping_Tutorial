using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = "Publisher, Author, Admin")]


    public class ShippingController : Controller
    {
        private readonly DataContext _context;

        public ShippingController(DataContext context)
        {
            _context = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var shippingList = await _context.Shippings.ToListAsync();
            ViewBag.ShippingList = shippingList;    
            return View();
        }

        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, double price)
        {
            shippingModel.Ward = phuong;
            shippingModel.District = quan;
            shippingModel.City = tinh;
            shippingModel.Price = price;

            try
            {
                var existed_shipping = await _context.Shippings.AnyAsync(s => s.Ward == phuong && s.District == quan && s.City == tinh);

                if (existed_shipping)
                    return Ok(new { duplicate = true, message = "Duplicated data" });

                _context.Shippings.Add(shippingModel);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Add shipping successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occur while processing");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var shipping = await _context.Shippings.FirstOrDefaultAsync(s => s.Id == id);

            _context.Shippings.Remove(shipping);
            await _context.SaveChangesAsync();
            TempData["success"] = "Delete shipping successfully";

            return RedirectToAction("Index", "Shipping");
        }
    }
}
