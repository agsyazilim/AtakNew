using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.IyzicoPay.Models
{
    public class ConfigurationModel:BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>The API key.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool Apikey_OverrideForStore { get; set; }
        /// <summary>
        /// Gets or sets the secret key.
        /// </summary>
        /// <value>The secret key.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.BaseUrl")]
        public string BaseUrl { get; set; }
        public bool BaseUrl_OverrideForStore { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ConfigurationModel"/> is popup.
        /// </summary>
        /// <value><c>true</c> if popup; otherwise, <c>false</c>.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.Popup")]

        public bool Popup { get; set; }
        public bool Popup_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the call back URL.
        /// </summary>
        /// <value>The call back URL.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.CallBackUrl")]
        public string CallBackUrl { get; set; }
        public bool CallBackUrl_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the additional fee.
        /// </summary>
        /// <value>The additional fee.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [additional fee percentage].
        /// </summary>
        /// <value><c>true</c> if [additional fee percentage]; otherwise, <c>false</c>.</value>
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.IyzicoPay.Fields.Installment")]
        public string Installments { get; set; }
        public bool Installments_OverrideForStore { get; set; }
    }
}
