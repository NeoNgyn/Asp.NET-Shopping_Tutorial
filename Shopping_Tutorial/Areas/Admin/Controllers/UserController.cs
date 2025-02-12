using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System.Security.Claims;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;

        public UserController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var userWithRoles = await (from u in _context.Users
                                       join ur in _context.UserRoles on u.Id equals ur.UserId
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       select new {User = u, RoleName = r.Name}).ToListAsync();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.loggedInUserId = loggedInUserId;
            return View(userWithRoles);
        }

        

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if(ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createdUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createdUser.Id;
                    var role = _roleManager.FindByIdAsync(user.RoleId);
                    var addToRoleResult = await _userManager.AddToRoleAsync(createdUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (IdentityError error in addToRoleResult.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (IdentityError error in createUserResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["Error"] = "Add brand failed!";
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
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if(string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var deletResult = await _userManager.DeleteAsync(user);
            if (!deletResult.Succeeded)
                return View("Error");

            TempData["success"] = "Delete successfully!";
            return RedirectToAction("Index");

        }

        [HttpGet]
        [Route("Update")]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Update")]
        public async Task<IActionResult> Update(AppUserModel updateUser, string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            

            if (ModelState.IsValid)
            {
                user.UserName = updateUser.UserName;
                user.Email = updateUser.Email;
                user.PhoneNumber = updateUser.PhoneNumber;
                user.RoleId = updateUser.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (updateUserResult.Succeeded)
                {
                    var createdUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createdUser.Id;
                    var role = _roleManager.FindByIdAsync(user.RoleId);
                    var addToRoleResult = await _userManager.AddToRoleAsync(createdUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (IdentityError error in addToRoleResult.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (IdentityError error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(updateUser);
                }
            }
            else
            {
                TempData["Error"] = "Update User failed!";
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

            return RedirectToAction("Index", "User");
        }
    }
}
