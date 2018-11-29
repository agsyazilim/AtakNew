using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.IyzicoPay.Models;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.IyzicoPay.Components
{
    [ViewComponent(Name = "PaymentIyzicoPay")]
    public class PaymentIyzicoPayViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;

        public PaymentIyzicoPayViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }


        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel
            {
                CreditCardTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Visa", Value = "VISA" },
                    new SelectListItem { Text = "Master Card", Value = "MASTER_CARD" },
                    new SelectListItem { Text = "American Express", Value = "AMERICAN_EXPRESS" },
                    new SelectListItem { Text = "Troy", Value = "TROY" },
                },
                CardFamilyNames = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Bonus", Value = "Bonus" },
                    new SelectListItem { Text = "Axess", Value = "Axess" },
                    new SelectListItem { Text = "World", Value = "World" },
                    new SelectListItem { Text = "Maximum", Value = "Maximum" },
                    new SelectListItem{Text = "Paraf",Value = "Paraf"},
                    new SelectListItem{Text = "CardFinans",Value = "CardFinans"},
                    new SelectListItem{Text = "Advantage",Value = "Advantage"},
                    new SelectListItem{Text = "Paracard",Value = "Paracard"},
                    new SelectListItem{Text = "Tlcard",Value = "TlCard"},
                    new SelectListItem{Text = "neo",Value = "Neo"}

                }

            };

            var instalment = _settingService.LoadSetting<IyzicoPayPaymentSettings>().Installments
                .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            foreach (var ins in instalment)
            {
                model.Installmentss.Add(new SelectListItem
                {
                    Text = ins,
                    Value = ins
                });
            }


            //years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem { Text = year, Value = year, });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem { Text = i.ToString("D2"), Value = i.ToString(), });
            }
            

            //set postback values (we cannot access "Form" with "GET" requests)
            if (this.Request.Method != WebRequestMethods.Http.Get)
            {
                var form = this.Request.Form;
                model.IdentityNumber = form["IdentityNumber"]; 
                model.CardNumber = form["CardNumber"];
                model.CardCode = form["CardCode"];
                var selectedCcType = model.CreditCardTypes.FirstOrDefault(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase));
                if (selectedCcType != null)
                    selectedCcType.Selected = true;
                var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
                if (selectedMonth != null)
                    selectedMonth.Selected = true;
                var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
                if (selectedYear != null)
                    selectedYear.Selected = true;
            }


            return View("~/Plugins/Payments.IyzicoPay/Views/PaymentInfo.cshtml", model);

        }
    }
}
