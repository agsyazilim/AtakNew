using System;
using Nop.Plugin.Payments.IyzicoPay.Iyzico.Request;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico.Models
{
    public class Refund : IyzipayResource
    {
        public String PaymentId { get; set; }
        public String PaymentTransactionId { get; set; }
        public String Price { get; set; }
        public String Currency { get; set; }
        public String ConnectorName { get; set; }

        public static Refund Create(CreateRefundRequest request, Options options)
        {
            return RestHttpClient.Create().Post<Refund>(options.BaseUrl + "/payment/refund", GetHttpHeaders(request, options), request);
        }
    }
}
