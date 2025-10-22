using Microsoft.AspNetCore.Mvc;

namespace Vex_E_commerce.ViewComponents
{
    public class ButtonViewComponents : ViewComponent
    {
        public IViewComponentResult Invoke(string label, string action, string controller, string color)
        {
            var model = new ButtonModel
            {
                label = label,
                Action = action,
                Controller = controller,
                Color = color
            };

            return View(model);
        }
    }

    public class ButtonModel
    {
        public string label { get; set; } = "";
        public string Action { get; set; } = "";
        public string Controller { get; set; } = "";
        public string Color { get; set; } = "";
    }
}
