using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.IyzicoPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.Payments.IyzicoPay.Success", "Plugins/IyzicoPay/Success",
                new { controller = "PaymentIyzicoPay", action = "Success" });

            routeBuilder.MapRoute("Plugin.Payments.IyzicoPay.BinControl", "Plugins/IyzicoPay/BinControl/{cardnumber}",
                new { controller = "PaymentIyzicoPay", action = "BinControl" });

        }

        public int Priority
        {
            get { return -1; }
        }
    }
}