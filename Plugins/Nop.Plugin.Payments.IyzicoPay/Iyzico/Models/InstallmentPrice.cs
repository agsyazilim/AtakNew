using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico
{
    public class InstallmentPrice
    {
        [JsonProperty(PropertyName = "InstallmentPrice")]
        public String Price { get; set; }
        public String TotalPrice { get; set; }
        public int? InstallmentNumber { get; set; }
    }
}
