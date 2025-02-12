using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Tutorial.Repository.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DataContext _context;

        public FooterViewComponent(DataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync() => View(await _context.Contacts.FirstOrDefaultAsync());
    }
}
