using System;
using Nop.Plugin.Payments.IyzicoPay.Iyzico;

namespace Nop.Plugin.Payments.IyzicoPay
{
    public class HelperApiOptions
    {

        /// <summary>
        /// Gets the API context.
        /// </summary>
        /// <param name="iyzicoSettings">The iyzico settings.</param>
        /// <returns>Options.</returns>
        public static Options GetApiContext(IyzicoPayPaymentSettings iyzicoSettings)
        {
            var options = new Options
            {
                ApiKey = $"{iyzicoSettings.ApiKey.Trim().Replace(" ", string.Empty)}",
                SecretKey = $"{iyzicoSettings.SecretKey.Trim().Replace(" ", String.Empty)}",
                BaseUrl = $"{iyzicoSettings.BaseUrl.Trim().Replace(" ", String.Empty)}"
            };
            return options;
        }

    }
}