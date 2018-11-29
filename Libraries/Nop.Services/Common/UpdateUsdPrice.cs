using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Services.Catalog;
using Nop.Services.Tasks;

namespace Nop.Services.Common
{
    public class UpdateUsdPrice : IScheduleTask
    {
        private IProductService _productService;

        public UpdateUsdPrice(IProductService productService)
        {
            _productService = productService;
        }

        public void Execute()
        {
            string today = "http://www.tcmb.gov.tr/kurlar/today.xml";
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
            string usd = xmlDoc.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteSelling").InnerXml;
            var usdStr = string.Format("{0:G29}", decimal.Parse(usd));
            var usdVal = Convert.ToDecimal(usdStr);
            var products = _productService.GetUsdProducts();
            foreach (var product in products)
            {
                var formatedCost = string.Format("{0:G29}", product.ProductCost);
                var desVal = Convert.ToDecimal(formatedCost);
                var tlPrice = desVal * usdVal;
                var manyCOst = Convert.ToDecimal(string.Format("{0:##.00}", tlPrice));
                product.Price = manyCOst;
                _productService.UpdateProduct(product);
            }
        }
    }
}
