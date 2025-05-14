using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ecommerce.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class TestModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}