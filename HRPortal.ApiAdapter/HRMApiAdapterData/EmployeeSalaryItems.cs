using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class SalaryItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string NameEN { get; set; }
    }

    public class EmployeeSalaryItems
    {
        public string SalaryFirstDay { get; set; }
        public string SalaryLastDay { get; set; }
        public List<SalaryItem> TaxableItems { get; set; }
        public List<SalaryItem> NoTaxItems { get; set; }
        public List<SalaryItem> WithholdingItems { get; set; }
        public List<SalaryItem> TaxableItemsTotal { get; set; }
        public List<SalaryItem> WithholdingItemsTotal { get; set; }
        public string LanguageCookie { get; set; }
    }

    public class EmployeeSalaryItemsDDMC
    {
        /// <summary>
        ///應稅項目陣列
        ///</summary>
        public TaxableItemDataFEDS TaxableItems { get; set; }
        /// <summary>
        ///免稅項目陣列
        ///</summary>
        public SalaryItemFEDS NoTaxItems { get; set; }
        /// <summary>
        ///免稅項目陣列(其他項)
        ///</summary>
        public SalaryItemFEDS NoTaxItemsOthers { get; set; }
        /// <summary>
        ///代扣項目陣列
        ///</summary>
        public SalaryItemFEDS WithholdingItems { get; set; }
    }

    ///<summary>
    ///項目金額物件
    ///</summary>
    public class SalaryItemsNameAmountDDMC
    {
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 金額
        /// </summary>
        public double Amount { get; set; }
    }

    ///<summary>
    ///大項目物件
    ///</summary>
    public class SalaryItemDDMC
    {
        /// <summary>
        /// 總合
        /// </summary>
        public double Total { get; set; }
        /// <summary>
        /// 細項陣列
        /// </summary>
        public List<SalaryItemsNameAmountFEDS> Detail { get; set; }
    }

    ///<summary>
    ///課稅項合計項目
    ///</summary>
    public class TaxableItemDataDDMC
    {
        /// <summary>
        /// 基本項
        /// </summary>
        public SalaryItemFEDS BasicItems { get; set; }

        /// <summary>
        /// 基本項(其他項)
        /// </summary>
        public SalaryItemFEDS BasicItemsOthers { get; set; }

        /// <summary>
        /// 加發項
        /// </summary>
        public SalaryItemFEDS AddItems { get; set; }

        /// <summary>
        /// 加發項(其他項)
        /// </summary>
        public SalaryItemFEDS AddItemsOthers { get; set; }

        /// <summary>
        /// 減發項
        /// </summary>
        public SalaryItemFEDS MinusItems { get; set; }

        /// <summary>
        /// 減發項(其他項)
        /// </summary>
        public SalaryItemFEDS MinusItemsOthers { get; set; }

        /// <sumary>
        /// 總合
        /// </sumary>
        public double Total { get; set; }

    }
}
