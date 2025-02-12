using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Tutorial.Repository.Components
{
    public class BrandsViewComponent : ViewComponent
    {
        public readonly DataContext _context;

        public BrandsViewComponent(DataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync() => View(await _context.Brands.ToListAsync());
    }
}
