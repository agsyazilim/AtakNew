﻿using System;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.IyzicoPay.Iyzico
{
    public class InstallmentDetail
    {
        public String BinNumber { get; set; }
        public String Price { get; set; }
        public String CardType { get; set; }
        public String CardAssociation { get; set; }
        public String CardFamilyName { get; set; }
        public int? Force3Ds { get; set; }
        public long? BankCode { get; set; }
        public String BankName { get; set; }
        public int? ForceCvc { get; set; }
        public List<InstallmentPrice> InstallmentPrices { get; set; }
    }
}
