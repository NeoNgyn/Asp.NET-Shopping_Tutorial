using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Contact")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {


        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public IActionResult Index()
        {
            var contact = _context.Contacts.ToList();
            return View(contact);
        }

        [Route("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContactModel updatedContact)
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync(y => y.Id == id);

            if (ModelState.IsValid)
            {

                contact.Name = updatedContact.Name;
                contact.Description = updatedContact.Description;
                contact.Map = updatedContact.Map;
                contact.Email = updatedContact.Email;
                contact.Phone = updatedContact.Phone;

                if (contact.ImageUpload != updatedContact.ImageUpload)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/contacts");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    contact.LogoImage = imageName;
                }

                if (contact.ImageUpload == null)
                    contact.ImageUpload = updatedContact.ImageUpload;

                _context.Contacts.Update(contact);
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
    }
}
