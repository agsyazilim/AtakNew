using System;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico
{
    public class CreateThreedsPaymentRequest : BaseRequest
    {
        public String PaymentId { get; set; }
        public String ConversationData { get; set; }

        public override String ToPKIRequestString()
        {
            return ToStringRequestBuilder.NewInstance()
                .AppendSuper(base.ToPKIRequestString())
                .Append("paymentId", PaymentId)
                .Append("conversationData", ConversationData)
                .GetRequestString();
        }
    }
}
