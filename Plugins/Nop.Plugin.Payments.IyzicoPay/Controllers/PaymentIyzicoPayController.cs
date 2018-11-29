
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.IyzicoPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using Nop.Plugin.Payments.IyzicoPay.Iyzico;
using Nop.Services.Directory;

namespace Nop.Plugin.Payments.IyzicoPay.Controllers
{
    public class PaymentIyzicoPayController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IyzicoPayPaymentSettings _iyzicoPayPaymentSettings;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;
        private readonly ICurrencyService _currencyService;

        #endregion Fields

        #region Ctor

        public PaymentIyzicoPayController(ILocalizationService localizationService, IPermissionService permissionService, ISettingService settingService, IStoreContext storeContext, IWorkContext workContext,
            IyzicoPayPaymentSettings iyzicoPayPaymentSettings, ICustomerService customerService,
            IPaymentService paymentService,IOrderService orderService,OrderSettings orderSettings, ICurrencyService currencyService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _iyzicoPayPaymentSettings = iyzicoPayPaymentSettings;
            _customerService = customerService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderSettings = orderSettings;
            _currencyService = currencyService;
        }

        #endregion Ctor

        #region Metod

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var iyzicoPaySettings = _settingService.LoadSetting<IyzicoPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                BaseUrl = iyzicoPaySettings.BaseUrl,
                SecretKey = iyzicoPaySettings.SecretKey,
                Popup = iyzicoPaySettings.Popup,
                ApiKey = iyzicoPaySettings.ApiKey,
                CallBackUrl = iyzicoPaySettings.CallBackUrl,
                AdditionalFee = iyzicoPaySettings.AdditionalFee,
                AdditionalFeePercentage = iyzicoPaySettings.AdditionalFeePercentage,
                Installments = iyzicoPaySettings.Installments
            };
            return View("~/Plugins/Payments.IyzicoPay/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;

            var iyzicoPaySettings = _settingService.LoadSetting<IyzicoPayPaymentSettings>(storeScope);
            iyzicoPaySettings.ApiKey = model.ApiKey;
            iyzicoPaySettings.SecretKey = model.SecretKey;
            iyzicoPaySettings.BaseUrl = model.BaseUrl;
            iyzicoPaySettings.Popup = model.Popup;
            iyzicoPaySettings.CallBackUrl = model.CallBackUrl;
            iyzicoPaySettings.AdditionalFee = model.AdditionalFee;
            iyzicoPaySettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            iyzicoPaySettings.Installments = model.Installments;

            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.ApiKey,
                model.Apikey_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.SecretKey,
                model.SecretKey_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.BaseUrl,
                model.BaseUrl_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.Popup,
                model.Popup_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.CallBackUrl,
                model.CallBackUrl_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.AdditionalFee,
                model.AdditionalFee_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.AdditionalFeePercentage,
                model.AdditionalFeePercentage_OverrideForStore, storeScope);
            _settingService.SaveSettingOverridablePerStore(iyzicoPaySettings, x => x.Installments,
                model.Installments_OverrideForStore, storeScope);
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
        [HttpPost]
        public IActionResult BinControl(string cardnumber)
        {
            var customer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            RetrieveInstallmentInfoRequest request = new RetrieveInstallmentInfoRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = customer.CustomerGuid.ToString();
            request.BinNumber = cardnumber;
            decimal price = 0;
            foreach (var shoppingCartItem in cart)
            {
                price += (shoppingCartItem.Product.Price * shoppingCartItem.Quantity);
            }
            request.Price = _currencyService.ConvertFromPrimaryStoreCurrency(price,_workContext.WorkingCurrency).ToString("##.###").Replace(',', '.');
            InstallmentInfo installmentInfo =
                InstallmentInfo.Retrieve(request, HelperApiOptions.GetApiContext(_iyzicoPayPaymentSettings));

            return Json(installmentInfo);
        }
       
        public IActionResult Success(IFormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName(IyzicoPayPaymentDefaults.SystemName) as IyzicoPayPaymentProcessor;
            if (processor == null ||!_paymentService.IsPaymentMethodActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("Iyzico  module cannot be loaded");
            var model = new SuccessModel();
            CreateThreedsPaymentRequest threquest = new CreateThreedsPaymentRequest();
            threquest.Locale = Locale.TR.ToString();
            threquest.PaymentId = form["paymentId"];
            threquest.ConversationData = form["conversationData"];
            threquest.ConversationId = form["conversationId"];
            ThreedsPayment threedsPayment = ThreedsPayment.Create(threquest,
                HelperApiOptions.GetApiContext(_iyzicoPayPaymentSettings));
            if (threedsPayment.Status == "success")
            {
                if (form["mdStatus"] == "1")
                {
                    var customer = _customerService.GetCustomerByGuid(new Guid(threedsPayment.ConversationId));
                    var query = _orderService.SearchOrders(customerId: customer.Id).ToList();
                    var order = query.FirstOrDefault();
                    order.PaymentStatus = threedsPayment.FraudStatus == 1 ? PaymentStatus.Paid : PaymentStatus.Pending;
                    order.OrderStatus = OrderStatus.Processing;
                    order.AuthorizationTransactionId = threedsPayment.PaymentId;
                    order.AuthorizationTransactionCode = threedsPayment.AuthCode;
                    order.PaidDateUtc = DateTime.UtcNow;
                    var paymentrequest = new ProcessPaymentRequest();
                    var ordernote = new OrderNote();
                    ordernote.DisplayToCustomer = false;
                    ordernote.CreatedOnUtc = DateTime.UtcNow;
                    ordernote.Note = "Fraud:" + threedsPayment.FraudStatus;
                   
                    paymentrequest.CustomValues.Add("fraudstatus", threedsPayment.FraudStatus);
                    foreach (var item in threedsPayment.PaymentItems)
                    {
                        ordernote.Note += string.Format("{0}{1}", item.ItemId, item.PaymentTransactionId); 
                    }
                    order.OrderNotes.Add(ordernote);
                    _orderService.UpdateOrder(order); ;
                    _orderService.UpdateOrder(order);
                    if (_orderSettings.OnePageCheckoutEnabled)
                        return RedirectToRoute("HomePage");
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else
                {
                    switch (form["mdStatus"])
                    {
                        case "0": model.Errorr = "3-D Secure imzası geçersiz veya doğrulama"; break;
                        case "2": model.Errorr = "Kart sahibi veya bankası sisteme kayıtlı değil"; break;
                        case "3": model.Errorr = "Kartın bankası sisteme kayıtlı değil"; break;
                        case "4": model.Errorr = "Doğrulama denemesi, kart sahibi sisteme daha sonra kayıt olmayı seçmiş"; break;
                        case "5": model.Errorr = "Doğrulama yapılamıyor"; break;
                        case "6": model.Errorr = "3-D Secure hatası"; break;
                        case "7": model.Errorr = "Sistem hatası"; break;
                        case "8": model.Errorr = "Bilinmeyen kart no"; break;
                        default: model.Errorr = "Hata Oluştu"; break;
                    }
                    return View("~/Plugins/Payments.IyzicoPay/Views/Success.cshtml", model);
                }
            }
            else
            {
                model.Errorr = threedsPayment.ErrorMessage;
                return View("~/Plugins/Payments.IyzicoPay/Views/Success.cshtml", model);
            }
        }

        #endregion Metod
    }
}