// Atilla KayaNopCommerceNop.ServicesNetsisTask.cs

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Nop.Services.Catalog;
using Nop.Services.Tasks;

namespace Nop.Services.Netsis
{
    public partial class NetsisTask : IScheduleTask
    {
        private const string StockQuery = "select cast( (coalesce ( (select sum(case when a.sthar_gckod = 'G' then a.sthar_gcmik else 0 end) from [ATAK2018].[dbo].TBLSTHAR a where stok_kodu in (select stok_kodu from [ATAK2018].[dbo].TBLSTSABIT c where rtrim(ltrim(c.stok_adi)) =  rtrim(ltrim(b.stok_adi)))),0) - coalesce ( (select sum(case when a.sthar_gckod = 'C' then a.sthar_gcmik else 0 end) from [ATAK2018].[dbo].TBLSTHAR a where stok_kodu in (select stok_kodu from [ATAK2018].[dbo].TBLSTSABIT c where rtrim(ltrim(c.stok_adi)) =  rtrim(ltrim(b.stok_adi)))),0)  ) as int) as stokKalan, rtrim(ltrim(b.stok_adi)) as stok_adi, SATIS_FIAT4,stok_kodu from [ATAK2018].[dbo].TBLSTSABIT b Join (SELECT [STOK_KODU] As sskod FROM [ATAK2018].[dbo].[TBLSTSABITEK] Where [INGISIM] = 'ATAKWEP') n2 On b.STOK_KODU = n2.sskod group by rtrim(ltrim(b.stok_adi)),SATIS_FIAT2,SATIS_FIAT4 ,stok_kodu";
        private readonly IProductService _productService;
        /// <summary>
        /// NetsisTask
        /// </summary>
        /// <param name="productService"></param>
        public NetsisTask(IProductService productService)
        {
            _productService = productService;
        }


        public static List<T> ConvertDataTableToGenericList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                   .Select(c => c.ColumnName)
                   .ToList();

            var properties = typeof(T).GetProperties();
            DataRow[] rows = dt.Select();
            return rows.Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name))
                        pro.SetValue(objT, row[pro.Name]);
                }

                return objT;
            }).ToList();
        }


        private void Sync()
        {

            var dt = ConvertDataTableToGenericList<NetsisModel>(SqlComm2.SqlDataTable(StockQuery));
            var netsisProductList = dt.Select(x => new NetsisModel
            {
                StockCode = x.StockCode,
                ProductName = ReplaceUnKnonwChars(x.ProductName),
                Price = x.Price,
                Stock = x.Stock
            }).ToList();

            foreach (var netsisModel in netsisProductList)
            {
                var product = _productService.GetProductByMpn(netsisModel.StockCode);
                if (product != null)
                {
                    product.Name = netsisModel.ProductName;
                    product.Price = Convert.ToDecimal(netsisModel.Price);
                    product.StockQuantity = Convert.ToInt32(netsisModel.Stock) >= 0 ? Convert.ToInt32(netsisModel.Stock) :0;
                    product.DisableBuyButton = Convert.ToInt32(netsisModel.Stock) <= 0?true:false;
                    _productService.UpdateProduct(product);
                }
            }
        }
        private static string ReplaceUnKnonwChars(string str)
        {
            return str.Replace("Ý", "İ").Replace("Þ", "Ş").Replace("ý", "ı").Replace("ð", "ğ").Replace("þ", "ş");
        }


        /// <summary>
        /// Execute
        /// </summary>
        public void Execute()
        {
            Sync();
        }
    }
}