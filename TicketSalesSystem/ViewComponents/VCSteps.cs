using Microsoft.AspNetCore.Mvc;

namespace TicketSalesSystem.ViewComponents
{
    public class VCSteps: ViewComponent
    {
        // currentStep 由 View 傳進來，決定哪一個數字要亮起來
        public IViewComponentResult Invoke(int currentStep)
        {
            return View(currentStep);
        }
    }
}
