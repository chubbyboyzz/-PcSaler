using Microsoft.AspNetCore.Mvc;

namespace PcSaler.ViewComponents
{
    public class SearchMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string currentQuery)
        {
            return View("Default", currentQuery);
        }
    }
}