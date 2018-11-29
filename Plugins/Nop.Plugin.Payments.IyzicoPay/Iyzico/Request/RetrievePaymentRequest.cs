using System;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico
{
   public class RetrievePaymentRequest : BaseRequest
    {
        public String PaymentId { get; set; }
        public String PaymentConversationId { get; set; }

        public override String ToPKIRequestString()
        {
            return ToStringRequestBuilder.NewInstance()
                .AppendSuper(base.ToPKIRequestString())
                .Append("paymentId", PaymentId)
                .Append("paymentConversationId", PaymentConversationId)             
                .GetRequestString();
        }
    }
}
