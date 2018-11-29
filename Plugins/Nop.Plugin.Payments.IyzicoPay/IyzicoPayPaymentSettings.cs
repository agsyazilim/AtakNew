using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.IyzicoPay
{
    /// <summary>
    /// Class IyzicoPayPaymentSettings.
    /// </summary>
    /// <seealso cref="Nop.Core.Configuration.ISettings" />
    public class IyzicoPayPaymentSettings:ISettings
    {
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; set; }
        /// <summary>
        /// Gets or sets the secret key.
        /// </summary>
        /// <value>The secret key.</value>
        public string SecretKey { get; set; }
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IyzicoPayPaymentSettings"/> is popup.
        /// </summary>
        /// <value><c>true</c> if popup; otherwise, <c>false</c>.</value>
        public bool Popup { get; set; }
        /// <summary>
        /// Gets or sets the additional fee.
        /// </summary>
        /// <value>The additional fee.</value>
        public decimal AdditionalFee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [additional fee percentage].
        /// </summary>
        /// <value><c>true</c> if [additional fee percentage]; otherwise, <c>false</c>.</value>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Gets or sets the call back URL.
        /// </summary>
        /// <value>The call back URL.</value>
        public string CallBackUrl { get; set; }

        public string Installments { get; set; }
       
    }
}