using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signinManager;
        private readonly IEmailSender _emailSender;
        public AccountController(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, DataContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _context = context;
            _emailSender = emailSender;
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl});
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginvm)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signinManager.PasswordSignInAsync(loginvm.Username, loginvm.Password, false, false);
                if (result.Succeeded)
                {
                    return Redirect(loginvm.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Invalid Username or Password");
            }
            return View(loginvm);
        }

        [HttpGet]
        [Route("LoginGoogle")]
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        [HttpGet]
        [Route("GoogleResponse")]
        public async Task<IActionResult>
         GoogleResponse()
        {
            // Authenticate using Google scheme
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                //Nếu xác thực ko thành công quay về trang Login
                return RedirectToAction("Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            //var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            string emailName = email.Split('@')[0];
            //return Json(claims);
            // Check user có tồn tại không
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                //nếu user ko tồn tại trong db thì tạo user mới với password hashed mặc định 1-9
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");
                //username thay khoảng cách bằng dấu "-" và chữ thường hết
                var newUser = new AppUserModel { UserName = emailName, Email = email };
                newUser.PasswordHash = hashedPassword; // Set the hashed password cho user

                var createUserResult = await _userManager.CreateAsync(newUser);
                if (!createUserResult.Succeeded)
                {
                    TempData["error"] = "Register account failed. Please try again!";
                    return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

                }
                else
                {
                    // Nếu user tạo user thành công thì đăng nhập luôn 
                    await _signinManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Register account successfully";
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                //Còn user đã tồn tại thì đăng nhập luôn với existingUser
                TempData["success"] = "Login account successfully";
                await _signinManager.SignInAsync(existingUser, isPersistent: false);
                return RedirectToAction("Index", "Home");

            }

            return RedirectToAction("Login");

        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
                IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "User created successfully!";
                    return RedirectToAction("Login");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signinManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl);
        }


        [HttpGet]
        [Route("UpdateAccount")]
        public async Task<IActionResult> UpdateAccount()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);    
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) 
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [Route("UpdateAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AppUserModel user)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userById = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();
            else
            {
                userById.PhoneNumber = user.PhoneNumber;
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);
                userById.PasswordHash = passwordHash;
                _context.Users.Update(userById);
                await _context.SaveChangesAsync();
                TempData["success"] = "Update account information successfully!";
                return View(userById);
            }
            return View();
        }

        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found!";
                return RedirectToAction("ForgotPassword", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                //update token for user
                checkMail.Token = token;
                _context.Update(checkMail);
                await _context.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Change password for user" + checkMail.Email;
                var message = "Click this link to reset password: " + $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?email=" + checkMail.Email + "&token=" + token;
                await _emailSender.SendEmailAsync(receiver, subject, message);

            }
            TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
            return RedirectToAction("ForgotPassword", "Account");

        }

        public async Task<IActionResult> ResetPassword(AppUserModel user, string token)
        {
            var checkUser = await _userManager.Users.FirstOrDefaultAsync<AppUserModel>(u => u.Email == user.Email && u.Token == token);

            if (checkUser != null)
            {
                ViewBag.Email = user.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or Token is not right";
                return RedirectToAction("ForgotPassword", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Token == token);

            if (checkUser != null)
            {
                //Update new Password and Token
                string newToken = Guid.NewGuid().ToString();
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkUser, user.PasswordHash);

                checkUser.PasswordHash = passwordHash;
                checkUser.Token = newToken;
                await _userManager.UpdateAsync(checkUser);
                TempData["success"] = "Update new password successfully!";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or Token is not right!";
                return RedirectToAction("ForgotPassword", "Account");
            }
            return View();
        }
        
        [HttpGet]
        [Route("OrderHistory")]
        public async Task<IActionResult> OrderHistory()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var orders = await _context.Orders.Where(u => u.Username == userEmail).OrderByDescending(u => u.Id).ToListAsync();

            ViewBag.UserEmail = userEmail;

            return View(orders);
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

        [HttpGet]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            try
            {
                var order = await _context.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();

                order.Status = 3;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("An error occur while cancel order");
            }
            return RedirectToAction("OrderHistory", "Account");
        }
    }
}
