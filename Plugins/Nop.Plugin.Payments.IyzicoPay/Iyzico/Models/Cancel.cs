using System;
using Nop.Plugin.Payments.IyzicoPay.Iyzico.Request;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico.Models
{
    public class Cancel : IyzipayResource
    {
        public String PaymentId { get; set; }
        public String Price { get; set; }
        public String Currency { get; set; }
        public String ConnectorName { get; set; }

        public static Cancel Create(CreateCancelRequest request, Options options)
        {
            return RestHttpClient.Create().Post<Cancel>(options.BaseUrl + "/payment/cancel", GetHttpHeaders(request, options), request);
        }
    }
}
