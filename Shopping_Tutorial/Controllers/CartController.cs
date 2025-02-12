using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly DataContext _context;

        public CartController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<CartItemModel> cartList = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            var cityCookie = Request.Cookies["City"];
            var districtCookie = Request.Cookies["District"];
            var wardCookie = Request.Cookies["Ward"];
            var addressCookie = Request.Cookies["Address"];

            var couponCode = Request.Cookies["CouponTitle"];

            double shippingPrice = 0;
            double city = 0;
            double district = 0;
            double ward = 0;
            string address = "";

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<double>(shippingPriceJson);
            }

            if (cityCookie != null && districtCookie != null && wardCookie != null && addressCookie != null)
            {
                var cityJson = cityCookie;
                var districtJson = districtCookie;
                var wardJson = wardCookie;
                var addressJson = addressCookie;

                city = JsonConvert.DeserializeObject<double>(cityJson);
                district = JsonConvert.DeserializeObject<double>(districtJson);
                ward = JsonConvert.DeserializeObject<double>(wardJson);
                address = JsonConvert.DeserializeObject<string>(addressJson);
            }

            ViewBag.City = city; 
            ViewBag.District = district; 
            ViewBag.Ward = ward;
            ViewBag.Address = address;

            CartItemViewModel cartVM = new()
            {
                CartItems = cartList,
                GrandTotal = cartList.Sum(x => x.Quantity * x.Price),
                ShippingCost = shippingPrice,
                CouponCode = couponCode
            };
            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Add(int id)
        {
            ProductModel product = await _context.Products.FindAsync(id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItems = cart.Where(x => x.ProductId == id).FirstOrDefault();
            if (cartItems == null)
            {
                cart.Add(new CartItemModel(product));

            }
            else
                cartItems.Quantity += 1;

            HttpContext.Session.SetJson("Cart", cart);
            TempData["Success"] = "Add to cart successfully!";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Increase(int id)
        {
            ProductModel product = await _context.Products.Where(p => p.Id == id).FirstOrDefaultAsync();

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();
            if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
            {
                ++cartItem.Quantity;
                TempData["success"] = "Increase quantity successfully";
            }
            else
            {
                cartItem.Quantity = cartItem.Quantity;
                TempData["success"] = "Increase quantity successfully";
            }

            HttpContext.Session.SetJson("Cart", cart);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Decrease(int id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();
            if (cartItem.Quantity > 1)
                --cartItem.Quantity;
            else
                cart.Remove(cartItem);

            if (cart.Count == 0)
                HttpContext.Session.Remove("Cart");
            else
                HttpContext.Session.SetJson("Cart", cart);


            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            cart.Remove(cartItem);

            await _context.SaveChangesAsync();

            if (cart.Count == 0)
                HttpContext.Session.Remove("Cart");
            else
                HttpContext.Session.SetJson("Cart", cart);

            TempData["Success"] = "Remove item successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Clear cart successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string tinh, string quan, string phuong, string address, double tinhVal, double quanVal, double phuongVal)
        {
            var existed_Shipping = await _context.Shippings.FirstOrDefaultAsync(s => s.City == tinh && s.District == quan && s.Ward == phuong);

            double shippingPrice = 0;

            if (existed_Shipping != null)
                shippingPrice = existed_Shipping.Price;
            else
                shippingPrice = 50000;

            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            var cityJson = JsonConvert.SerializeObject(tinhVal);
            var districtJson = JsonConvert.SerializeObject(quanVal);
            var wardJson = JsonConvert.SerializeObject(phuongVal);
            var addressJson = JsonConvert.SerializeObject(address);

            try
            {
                CookieOptions cookie = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookie);
                Response.Cookies.Append("City", cityJson, cookie);
                Response.Cookies.Append("District", districtJson, cookie);
                Response.Cookies.Append("Ward", wardJson, cookie);
                Response.Cookies.Append("Address", addressJson, cookie);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }
            return Json(new { shippingPrice });
        }

        [HttpGet]
        [Route("Cart/DeleteShipping")]
        public IActionResult DeleteShipping()
        {
            Response.Cookies.Delete("ShippingPrice");
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        [Route("GetCoupon")]
        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            var validCoupon = await _context.Coupons.FirstOrDefaultAsync(p => p.Name == coupon_value && p.Quantity >= 1);

            string couponTitle = validCoupon.Name + " | " + validCoupon?.Description;

            if (validCoupon != null)
            {
                TimeSpan remainingTime = validCoupon.DateEnd - DateTime.Now;
                int daysRemaining = remainingTime.Days;

                if (daysRemaining >= 0)
                {
                    try
                    {
                        CookieOptions cookie = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                            Secure = true,
                            SameSite = SameSiteMode.Strict, //ktra tinh tuong thich cua trinh duyet
                        };

                        Response.Cookies.Append("CouponTitle", couponTitle, cookie);
                        return Ok(new { success = true, message = "Coupon applied successfully!" });
                    }
                    catch (Exception ex)
                    {
                        return Ok(new { success = false, message = "Coupon applied failed!" });
                    }
                }
                else
                    return Ok(new { success = false, message = "Coupon expired!" });
            }
            else
                return Ok(new { success = false, message = "Coupon not existed!" });

            return Json(new { CouponTitle = couponTitle });
        }

    }
}
