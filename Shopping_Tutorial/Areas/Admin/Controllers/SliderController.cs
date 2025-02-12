using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Slider")]
    [Authorize(Roles = "Publisher, Admin")]
    public class SliderController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;  
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<SliderModel> slider = _context.Sliders.ToList();

            const int pageSize = 10;

            if (pg < 1)
                pg = 1;

            int recsCount = slider.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = slider.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }

        [Route("Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if(ModelState.IsValid)
            {
                if(slider.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }

                _context.Sliders.Add(slider);
                await _context.SaveChangesAsync();
                TempData["success"] = "Create slider successfully!";
                return RedirectToAction("Index", "Slider");
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
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(x => x.Id == id);
            return View(slider);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public async Task<IActionResult> Edit(int id, SliderModel updatedSlider)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(x => x.Id == id);

            if(ModelState.IsValid)
            {
                slider.Name = updatedSlider.Name;
                slider.Description = updatedSlider.Description;
                slider.Status = updatedSlider.Status;

                if(slider.ImageUpload != updatedSlider.ImageUpload)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }

                if(slider.ImageUpload == null)
                    slider.ImageUpload = updatedSlider.ImageUpload;

                _context.Sliders.Update(slider);
                await _context.SaveChangesAsync();
                TempData["success"] = "Update slider successfully!";
                return RedirectToAction("Index", "Slider");
            }
            else
            {
                TempData["Error"] = "Update slider failed!";
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

        [Route("Delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(p => p.Id == id);

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
            string oldImage = Path.Combine(uploadDir, slider.Image);

            if (System.IO.File.Exists(oldImage))
                System.IO.File.Delete(oldImage);

            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();
            TempData["success"] = "Delete slider successfully!";
            return RedirectToAction("Index");
        }
    }
}
