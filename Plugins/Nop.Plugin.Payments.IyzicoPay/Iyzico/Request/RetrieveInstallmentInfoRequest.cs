using System;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico
{
    public class RetrieveInstallmentInfoRequest : BaseRequest
    {
        public String BinNumber { get; set; }
        public String Price { get; set; }

        public override String ToPKIRequestString()
        {
            return ToStringRequestBuilder.NewInstance()
                .AppendSuper(base.ToPKIRequestString())
                .Append("binNumber", BinNumber)
                .AppendPrice("price", Price)
                .GetRequestString();
        }
    }
}
