﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.IyzicoPay.Models
{
    public class PaymentInfoModel:BaseNopModel
    {
        public PaymentInfoModel()
        {
            CreditCardTypes = new List<SelectListItem>();
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
            CardFamilyNames = new List<SelectListItem>();
            Installmentss = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Payment.IdentityNumber")]
        public string IdentityNumber { get; set; }
        [NopResourceDisplayName("Payment.SelectCreditCard")]
        public string CreditCardType { get; set; }

        [NopResourceDisplayName("Payment.SelectCreditCard")]
        public IList<SelectListItem> CreditCardTypes { get; set; }
        [NopResourceDisplayName("Payment.CardFamilyName")]
        public string CardFamilyName { get; set; }

        public IList<SelectListItem> CardFamilyNames { get; set; }

        [NopResourceDisplayName("Payment.CardholderName")]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("Payment.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireMonth { get; set; }

        [NopResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireYear { get; set; }

        public IList<SelectListItem> ExpireMonths { get; set; }

        public IList<SelectListItem> ExpireYears { get; set; }

        [NopResourceDisplayName("Payment.CardCode")]
        public string CardCode { get; set; }

        public string Installments { get; set; }
        public IList<SelectListItem> Installmentss { get; set; }
      







    }
}
