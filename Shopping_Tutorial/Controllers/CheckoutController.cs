using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Momo;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers 
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private IMomoService _momoService;
        private readonly IVnpayService _vnpayService;
        public CheckoutController(DataContext context, IEmailSender emailSender, IVnpayService vnpayService, IMomoService momoService)
        {
            _context = context;
            _emailSender = emailSender;
            _vnpayService = vnpayService;
            _momoService = momoService;
        }

        public async Task<IActionResult> Checkout(string OrderId, string address = "", string tinh = "", string quan = "", string phuong = "")
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
                return RedirectToAction("Login", "Account");
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];

                var couponCode = Request.Cookies["CouponTitle"];

                double shippingPrice = 0;
                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<double>(shippingPriceJson);
                }
                orderItem.ShippingCost = shippingPrice;
                orderItem.OrderCode = orderCode;
                orderItem.CouponCode = couponCode;
                orderItem.Username = userEmail;
                orderItem.Status = 1;
                orderItem.CreatedDate = DateTime.Now;
                orderItem.Address = address + ", " + phuong + ", " + quan + ", " + tinh;
                if (OrderId != null)
                    orderItem.PaymentMethod = OrderId;
                else
                    orderItem.PaymentMethod = "COD";

                _context.Add(orderItem);
                _context.SaveChanges();
                List<CartItemModel> cartList = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cartItem in cartList)
                {
                    var orderDetails = new OrderDetail();
                    orderDetails.Username = userEmail;
                    orderDetails.OrderCode = orderCode;
                    orderDetails.ProductId = cartItem.ProductId;
                    orderDetails.Price = cartItem.Price;
                    orderDetails.Quantity = cartItem.Quantity;

                    var product = await _context.Products.Where(p => p.Id == cartItem.ProductId).FirstOrDefaultAsync();
                    product.Quantity -= cartItem.Quantity;
                    product.SoldQuantity += cartItem.Quantity;
                    _context.Products.Update(product);

                    _context.Add(orderDetails);
                    _context.SaveChanges();
                }
                HttpContext.Session.Remove("Cart");
                var receiver = "nguyenphucnhan2004@gmail.com";
                var subject = "Checkout in device successfully!";
                var message = "Checkout successfull! Enjoy the services";

                await _emailSender.SendEmailAsync(receiver, subject, message);
                TempData["success"] = "Checkout successfully";
                return RedirectToAction("OrderHistory", "Account");
            }
            return View("");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBackMoMo(MomoInfoModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;

            if (requestQuery["resultCode"] == 0)
            {
                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = double.Parse(requestQuery["Amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now,
                };
                _context.MomoInfos.Add(newMomoInsert);
                await _context.SaveChangesAsync();
                
                //Proceed to prepare order after checkout momo
                await Checkout("MOMO_" + requestQuery["orderId"]);
            }
            else
            {
                TempData["success"] = "Momo checkout cancelled!";
                return RedirectToAction("Index", "Cart");
            }

            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay(VnpayInfoModel model)
        {
            var response = _vnpayService.PaymentExecute(Request.Query);
            

            string desc = response.OrderDescription;


            if (response.VnPayResponseCode == "00")
            {
                var newVnpayInsert = new VnpayInfoModel
                {
                    OrderId = response.OrderId,
                    OrderDescription = response.OrderDescription,
                    Name = desc.Split(' ')[0],
                    Amount = double.Parse(desc.Split(' ').Last()),
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    PaymentMethod = response.PaymentMethod,
                    DatePaid = DateTime.Now
                };
                _context.VnpayInfos.Add(newVnpayInsert);
                await _context.SaveChangesAsync();

                //Proceed to prepare order after checkout momo
                await Checkout(newVnpayInsert.PaymentMethod.ToUpper() + "_" + response.OrderId);
                return View(newVnpayInsert);

            }
            else
            {
                TempData["success"] = "Vnpay checkout cancelled!";
                return RedirectToAction("Index", "Cart");
            }
            return View(response);
        }
    }
}
