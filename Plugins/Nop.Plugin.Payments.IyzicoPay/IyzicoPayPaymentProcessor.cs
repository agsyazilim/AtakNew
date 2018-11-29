using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.IyzicoPay.Controllers;
using Nop.Plugin.Payments.IyzicoPay.Iyzico;
using Nop.Plugin.Payments.IyzicoPay.Iyzico.Models;
using Nop.Plugin.Payments.IyzicoPay.Iyzico.Request;
using Nop.Plugin.Payments.IyzicoPay.Models;
using Nop.Plugin.Payments.IyzicoPay.Validators;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Currency = Nop.Plugin.Payments.IyzicoPay.Iyzico.Models.Currency;

namespace Nop.Plugin.Payments.IyzicoPay
{
    public class IyzicoPayPaymentProcessor : BasePlugin, IPaymentMethod
    {

        #region Field
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IyzicoPayPaymentSettings _iyzicoPayPaymentSettings;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICategoryService _categoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion
        #region Ctor
        public IyzicoPayPaymentProcessor(ILocalizationService localizationService, IOrderTotalCalculationService orderTotalCalculationService, ISettingService settingService, IWebHelper webHelper, IyzicoPayPaymentSettings iyzicoPayPaymentSettings, IStoreContext storeContext, ICustomerService customerService, IWorkContext workContext, TaxSettings taxSettings, ICurrencyService currencyService, ICategoryService categoryService,  IHttpContextAccessor httpContextAccessor,    IPaymentService paymentService, IGenericAttributeService genericAttributeService)
        {
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _iyzicoPayPaymentSettings = iyzicoPayPaymentSettings;
            _storeContext = storeContext;
            _customerService = customerService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _currencyService = currencyService;
            _categoryService = categoryService;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
            _genericAttributeService = genericAttributeService;
        }
        #endregion

        #region Utilites
        private ProcessPaymentResult ProcessPayment(ProcessPaymentRequest paymentRequest, bool isRecurringPayment)
        {
            var result = new ProcessPaymentResult();
            var iyzicoSettings = _settingService.LoadSetting<IyzicoPayPaymentSettings>();
            var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
            
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                 .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                 .LimitPerStore(_storeContext.CurrentStore.Id)
                 .ToList();
            var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax, out decimal orderSubTotalDiscountAmountBase, out List<DiscountForCaching> _, out decimal subTotalWithoutDiscountBase, out decimal _);

            var subtotalBase = subTotalWithoutDiscountBase;
            var subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
           
            CreatePaymentRequest request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = customer.CustomerGuid.ToString(),
                Price = subtotal.ToString("##.###").Replace(',', '.')
            };
            var paidprice = paymentRequest.CustomValues["Installments"].ToString();
            var instalment = paymentRequest.CustomValues["InstalmentCount"].ToString();
            var forceDs = false;
            if (!string.IsNullOrEmpty(paymentRequest.CustomValues["force3ds"].ToString()))
            {
                forceDs = paymentRequest.CustomValues["force3ds"].ToString() == "1" ? true : false;
            }

            request.PaidPrice = paidprice;
            request.Currency = _workContext.WorkingCurrency.CurrencyCode.ToString();
            if (!string.IsNullOrEmpty(instalment))
            {
                request.Installment = Convert.ToInt32(instalment);
            }

            request.BasketId = customer.Id.ToString();
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard
            {
                CardHolderName = paymentRequest.CreditCardName,
                CardNumber = paymentRequest.CreditCardNumber,
                ExpireMonth = paymentRequest.CreditCardExpireMonth.ToString(),
                ExpireYear = paymentRequest.CreditCardExpireYear.ToString(),
                Cvc = paymentRequest.CreditCardCvv2,
                RegisterCard = 0
            };
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer
            {
                Id = customer.Id.ToString(),
                Name =
                    _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute) ??
                    customer.BillingAddress.FirstName,
                Surname =
                    _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute) ??
                    customer.BillingAddress.LastName,
                GsmNumber =
                    _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute) ??
                    customer.BillingAddress.PhoneNumber,
                Email = customer.Email ?? customer.BillingAddress.Email,
                IdentityNumber = paymentRequest.CustomValues["IdentityNumber"].ToString() ??
                                 _genericAttributeService.GetAttribute<string>(customer,
                                     NopCustomerDefaults.VatNumberAttribute),
                LastLoginDate = Convert.ToDateTime(customer.LastLoginDateUtc).ToString("yyyy-M-d hh:mm:ff"),
                RegistrationDate = customer.CreatedOnUtc.ToString("yyyy-M-d  hh:mm:ff"),
                RegistrationAddress = customer.BillingAddress.Address1,
                Ip = customer.LastIpAddress,
                City = customer.BillingAddress.City,
                Country = customer.BillingAddress.Country.Name,
                ZipCode = ""
            };
            request.Buyer = buyer;

            Address shippingAddress = new Address
            {
                ContactName = customer.ShippingAddress.FirstName + " " + customer.ShippingAddress.LastName,
                City = customer.ShippingAddress.City,
                Country = customer.ShippingAddress.Country.Name,
                Description = customer.ShippingAddress.Address1,
                ZipCode = ""
            };
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address
            {
                ContactName = customer.BillingAddress.FirstName + " " + customer.BillingAddress.LastName,
                City = customer.BillingAddress.City,
                Country = customer.BillingAddress.Country.Name,
                Description = customer.BillingAddress.Address1,
                ZipCode = ""
            };
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();

            foreach (var cartItem in cart)
            {
                var categorys = _categoryService.GetProductCategoriesByProductId(cartItem.ProductId);
                var categoryName = string.Empty;
                foreach (var category in categorys)
                {
                    if (_categoryService.FindProductCategory(categorys,cartItem.ProductId, category.CategoryId) != null)
                        categoryName = category.Category.Name;
                }

                var basketItem = new BasketItem
                {
                    Id = cartItem.Id.ToString(),
                    Name = cartItem.Product.Name,
                    Category1 = categoryName,
                    Category2 = "",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (_currencyService.ConvertFromPrimaryStoreCurrency(
                            (cartItem.Product.Price * cartItem.Quantity), _workContext.WorkingCurrency))
                        .ToString("##.###")
                        .Replace(',', '.')
                };
                basketItems.Add(basketItem);
            }
            request.BasketItems = basketItems;
            if (forceDs)
            {
                request.CallbackUrl = $"{_webHelper.GetStoreLocation()}PaymentIyzicoPay/Success";
                ThreedsInitialize threedsInitialize = ThreedsInitialize.Create(request, HelperApiOptions.GetApiContext(iyzicoSettings));
                if (threedsInitialize.Status == "success")
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(threedsInitialize.HtmlContent);
                    this._httpContextAccessor.HttpContext.Response.Clear();
                    this._httpContextAccessor.HttpContext.Response.Body.Write(bytes, 0, bytes.Length);
                    this._httpContextAccessor.HttpContext.Response.Body.Close();
                    result.AllowStoringCreditCardNumber = true;
                  
                }
                else
                {
                    
                    result.Errors = new List<string> { threedsInitialize.ErrorCode, threedsInitialize.ErrorGroup, threedsInitialize.ErrorMessage };
                }

            }
            else
            {
                Payment payment = Payment.Create(request, HelperApiOptions.GetApiContext(iyzicoSettings));
                if (payment.Status == "success")
                {
                    result.NewPaymentStatus = payment.FraudStatus == 1 ? PaymentStatus.Paid : PaymentStatus.Pending;
                    result.AuthorizationTransactionId = payment.PaymentId;
                    result.AuthorizationTransactionCode = payment.AuthCode;
                    result.AllowStoringCreditCardNumber = true;
                    var trans = string.Empty;
                    foreach (var item in payment.PaymentItems)
                    {
                        trans += item.ItemId + "-" + item.PaymentTransactionId + "-";

                    }
                    result.AuthorizationTransactionResult = trans;
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Pending;
                    result.Errors = new List<string> { payment.ErrorCode, payment.ErrorGroup, payment.ErrorMessage };
                }
            }

            return result;
        }
        #endregion
        #region Metod

        /// <summary>
        /// Bir ödemeyi işleme koyma
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        /// <exception cref="ArgumentException">processPaymentRequest</exception>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return ProcessPayment(processPaymentRequest, false);
        }

        /// <summary>
        /// İşlem sonrası ödeme (üçüncü taraf URL'ye yönlendirmeyi gerektiren ödeme ağ geçitleri tarafından kullanılır)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {


        }
        /// <summary>
        /// Ödeme sırasında ödeme yönteminin gizlenip gizlenemeyeceğini gösteren 
        /// bir değeri döndürür
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        /// <summary>
        /// Ek taşıma ücreti getiriyor
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = _paymentService.CalculateAdditionalFee(cart,
                _iyzicoPayPaymentSettings.AdditionalFee,
                _iyzicoPayPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        ///  Ödeme Sonucu Yakalama
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Ödeme Talebi İadesi
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var refundResult = new RefundPaymentResult();
            if (!refundPaymentRequest.IsPartialRefund)
            {
                CreateCancelRequest request = new CreateCancelRequest();
                request.ConversationId = refundPaymentRequest.Order.CustomOrderNumber;
                request.Locale = Locale.TR.ToString();
                request.PaymentId = refundPaymentRequest.Order.AuthorizationTransactionId;
                request.Ip = refundPaymentRequest.Order.CustomerIp;
                Cancel cancel = Cancel.Create(request, HelperApiOptions.GetApiContext(_iyzicoPayPaymentSettings));
                if (cancel.Status == "success")
                {
                    refundResult.NewPaymentStatus = PaymentStatus.Refunded;
                    
                }
                else
                {
                    refundResult.AddError(cancel.ErrorGroup);
                }
            }
            else
            {
                string[] list = refundPaymentRequest.Order.AuthorizationTransactionResult.Split('-');
                CreateRefundRequest request = new CreateRefundRequest();
                request.ConversationId = refundPaymentRequest.Order.CustomOrderNumber;
                request.Locale = Locale.TR.ToString();
                request.PaymentTransactionId = list[0];
                request.Price = refundPaymentRequest.AmountToRefund.ToString("##.###").Replace(',','.');
                request.Ip = refundPaymentRequest.Order.CustomerIp;
                request.Currency = Currency.TRY.ToString();
                Iyzico.Models.Refund refund = Iyzico.Models.Refund.Create(request, HelperApiOptions.GetApiContext(_iyzicoPayPaymentSettings));
                if (refund.Status == "success")
                {
                    refundResult.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
                }
            }

            return refundResult;
        }

        /// <summary>
        /// Ödeme İptal Ödemesi
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Kartın Saklanıp bir sonraki ödemede Tekrar İşlemi
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        /// <exception cref="ArgumentException">processPaymentRequest</exception>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));
           

            return ProcessPayment(processPaymentRequest);
        }

        /// <summary>
        /// Tekrarlayan bir ödemeyi iptal eder
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentException">cancelPaymentRequest</exception>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest == null)
                throw new ArgumentException(nameof(cancelPaymentRequest));
            
            //always success
            return new CancelRecurringPaymentResult();
        }

        /// <summary>
        /// Müşteri, sipariş verildikten sonra tamamlanmadığında (ödeme yönlendirme yöntemleri için) bir ödemeyi tamamlayıp tamamlayamayacağını gösteren bir değeri alır.
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        /// <exception cref="ArgumentNullException">order</exception>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return false;
        }
        public Type GetControllerType()
        {
            return typeof(PaymentIyzicoPayController);
        }
        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();
            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"],
                IdentityNumber = form["IdentityNumber"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                    warnings.Add(error.ErrorMessage);
            return warnings;
        }

        /// <summary>
        /// Ödeme bilgileri alın
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var processpayment = new ProcessPaymentRequest();
            processpayment.CreditCardType = form["CreditCardType"];
            processpayment.CreditCardName = form["CardholderName"];
            processpayment.CreditCardNumber = form["CardNumber"];
            processpayment.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
            processpayment.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
            processpayment.CreditCardCvv2 = form["CardCode"];
            string installments = form["Installments"];
            string[] line = installments.Split('-').ToArray();
            string[] force3Ds = form["force3ds"].ToArray();
            processpayment.CustomValues.Add("force3ds", force3Ds[0]);
            processpayment.CustomValues.Add("Installments",line[0]);
            processpayment.CustomValues.Add("InstalmentCount",line[1]);
            processpayment.CustomValues.Add("IdentityNumber",form["IdentityNumber"]);
            return processpayment;
        }

        public string GetPublicViewComponentName()
        {
            return "PaymentIyzicoPay";
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentIyzicoPay/Configure";
        }

       

        public override void Install()
        {
            var settings = new IyzicoPayPaymentSettings
            {
                ApiKey = "sandbox-VQM1Anw3zsWfx5g1YeypO9095gKQ83Ea",
                SecretKey = "sandbox-pUj1y0QNA4cmiDprNDEuXc3iHxaRFuYa",
                BaseUrl = "https://sandbox-api.iyzipay.com",
                Popup = false,
                AdditionalFee = 0,
                AdditionalFeePercentage = false,
                Installments = "0"
            };
            _settingService.SaveSetting(settings);
            _localizationService.AddOrUpdatePluginLocaleResource("plugins.payments.iyzicopay.fields.apikey", "Api Key");
            _localizationService.AddOrUpdatePluginLocaleResource("plugins.payments.iyzicopay.fields.apikey.hint",
                "İyzico'dan Aldığınız ApiKey Bilgisiniz Giriniz");
            _localizationService.AddOrUpdatePluginLocaleResource("plugins.payments.iyzicopay.fields.baseurl", "Api Url");
            _localizationService.AddOrUpdatePluginLocaleResource("plugins.payments.iyzicopay.fields.baseurl.Hint",
                "İyzico'dan aldığınız Bağlantı Bilgisi Girilecek Test Ortamı Urlsi Sandbox ile başlar Gerçek Ortamda Kullanılmaz");
            _localizationService.AddOrUpdatePluginLocaleResource("plugins.payments.iyzicopay.fields.secretkey","Güvenlik Anahtarı");
            _localizationService.AddOrUpdatePluginLocaleResource("plugins.payments.iyzicopay.fields.secretkey.hint",
                "Güvenlik Anahtarı İyzico'dan Alınız");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.IyzicoPay.Fields.Popup","Popup");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.IyzicoPay.Fields.Popup.hint","İşaretlenirse Ödeme açılan pencereden yapılır.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.IdentityNumber","TC/Vergi No");
            base.Install();

        }

        public override void Uninstall()
        {
            var settings = _settingService.LoadSetting<IyzicoPayPaymentSettings>();
            if (!string.IsNullOrEmpty(settings.ApiKey))
                _settingService.DeleteSetting<IyzicoPayPaymentSettings>();
            if (!string.IsNullOrEmpty(settings.SecretKey))
                _settingService.DeleteSetting<IyzicoPayPaymentSettings>();
            if (!string.IsNullOrEmpty(settings.BaseUrl))
                _settingService.DeleteSetting<IyzicoPayPaymentSettings>();
            _localizationService.DeletePluginLocaleResource("plugins.payments.iyzicopay.fields.apikey");
            _localizationService.DeletePluginLocaleResource("plugins.payments.iyzicopay.fields.apikey.hint");
            _localizationService.DeletePluginLocaleResource("plugins.payments.iyzicopay.fields.baseurl");
            _localizationService.DeletePluginLocaleResource("plugins.payments.iyzicopay.fields.baseurl.hint");
            _localizationService.DeletePluginLocaleResource("plugins.payments.iyzicopay.fields.secretkey");
            _localizationService.DeletePluginLocaleResource("plugins.payments.iyzicopay.fields.secretkey.hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.IyzicoPay.Fields.Popup");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.IyzicoPay.Fields.Popup.hint");

            base.Uninstall();
        }

        public bool SupportCapture
        {
            get { return false; }
        }
        public bool SupportPartiallyRefund
        {
            get { return true; }
        }
        public bool SupportRefund
        {
            get { return true; }
        }
        public bool SupportVoid
        {
            get { return false; }
        }
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Standard; }
        }
        public bool SkipPaymentInfo
        {
            get { return false; }
        }
        public string PaymentMethodDescription { get; }
        #endregion
    }

}