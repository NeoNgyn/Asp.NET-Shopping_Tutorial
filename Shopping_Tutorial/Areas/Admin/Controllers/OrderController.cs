using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DataContext _context;
        public OrderController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }

        [HttpGet]
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailsOrder = await _context.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();

            var Order = _context.Orders.Where(o => o.OrderCode == ordercode).First();

            ViewBag.ShippingCost = Order.ShippingCost;

            ViewBag.Status = Order.Status;
            return View(DetailsOrder);
        }

        [HttpGet]
        [Route("PaymentInfoMomo")]
        public async Task<IActionResult> PaymentInfoMomo(string orderid)
        {
            var momoInfo = await _context.MomoInfos.FirstOrDefaultAsync(m => m.OrderId == orderid);

            if (momoInfo == null) 
                return NotFound();

            return View(momoInfo);
        }

        [HttpGet]
        [Route("PaymentInfoVnpay")]
        public async Task<IActionResult> PaymentInfoVnpay(string orderid)
        {
            var vnpayInfo = await _context.VnpayInfos.FirstOrDefaultAsync(m => m.OrderId == orderid);

            if (vnpayInfo == null)
                return NotFound();

            return View(vnpayInfo);
        }

        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
                return NotFound();

            order.Status = status;
            _context.Orders.Update(order);

            if (status == 0)
            {
                //Get data order detail base on order.OrderCode
                var detailsOrder = await _context.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == order.OrderCode)
                    .Select(od => new
                    {
                        od.Quantity,
                        od.Product.Price,
                        od.Product.CapitalPrice
                    }).ToListAsync();

                //get statistic data base on order date
                var statisticalModel = await _context.Statics.FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date);
                if (statisticalModel != null)
                {
                    foreach (var orderDetail in detailsOrder)
                    {
                        statisticalModel.Quantity += 1;
                        statisticalModel.Sold += orderDetail.Quantity;
                        statisticalModel.Revenue += orderDetail.Quantity * orderDetail.Price;
                        statisticalModel.Profit += orderDetail.Price - orderDetail.CapitalPrice;
                    }
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    double new_profit = 0;
                    foreach (var orderDetail in detailsOrder)
                    {
                        new_quantity += 1;
                        new_sold += orderDetail.Quantity;
                        new_profit += orderDetail.Price - orderDetail.CapitalPrice;

                        statisticalModel = new StatisticalModel
                        {
                            DateCreated = order.CreatedDate,
                            Quantity = new_quantity,
                            Sold = new_sold,
                            Revenue = orderDetail.Quantity + orderDetail.Price,
                            Profit = new_profit
                        };
                    }
                    _context.Add(statisticalModel);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {


                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
    }
}
