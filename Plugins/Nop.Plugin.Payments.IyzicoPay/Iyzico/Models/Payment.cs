using Nop.Plugin.Payments.IyzicoPay.Iyzico.Request;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico.Models
{
    public class Payment : PaymentResource
    {
        public static Payment Create(CreatePaymentRequest request, Options options)
        {
            return RestHttpClient.Create().Post<Payment>(options.BaseUrl + "/payment/auth", GetHttpHeaders(request, options), request);
        }

        public static Payment Retrieve(RetrievePaymentRequest request, Options options)
        {
            return RestHttpClient.Create().Post<Payment>(options.BaseUrl + "/payment/detail", GetHttpHeaders(request, options), request);
        }
    }
}
