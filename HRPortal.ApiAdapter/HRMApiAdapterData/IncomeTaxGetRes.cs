using System;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    /// <summary>
    /// 各類所得扣繳暨免扣繳憑單
    /// </summary>
    public class IncomeTaxGetRes
    {
        public IncomeTaxAreaInfo IncomeTaxArea { get; set; }

        public IncomeTaxTypeInfo IncomeTaxType { get; set; }

        public IncomeTaxPersonInfo IncomeTaxPerson { get; set; }

        public IncomeTaxTransInfo IncomeTaxTrans { get; set; }
    }

    /// <summary>
    /// 扣繳單位
    /// </summary>
    public class IncomeTaxAreaInfo
    {
        /// <summary>
        /// 統一編號
        /// </summary>
        public String UnifiedNo { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public String CompanyName { get; set; }
        /// <summary>
        /// 公司地址
        /// </summary>
        public String TaxAddress { get; set; }

        /// <summary>
        /// 扣繳義務人
        /// </summary>
        public String WithholdingAgent { get; set; }

        /// <summary>
        /// 機關代碼
        /// </summary>
        public String TaxAuthID { get; set; }
    }

    /// <summary>
    /// 格式代號及所得類別
    /// </summary>
    public class IncomeTaxTypeInfo
    {
        /// <summary>
        /// 所得格式代號
        /// </summary>
        public String TaxType { get; set; }
        /// <summary>
        /// 所得格式類別
        /// </summary>
        public String TaxTypeCode { get; set; }
        /// <summary>
        /// 所得標記
        /// </summary>
        public String RemarkFlag { get; set; }
        /// <summary>
        /// 所得類別敘述
        /// </summary>
        public String TaxDesc { get; set; }
        /// <summary>
        /// 所得類別序號代碼
        /// </summary>
        public String SerialCode { get; set; }
    }

    /// <summary>
    /// 所得人姓名或單位資訊
    /// </summary>
    public class IncomeTaxPersonInfo
    {
        /// <summary>
        /// 所得人姓名或單位名稱
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 國民身分證統一編號 or 所得單位統一編號
        /// </summary>
        public String IDNumber { get; set; }
        /// <summary>
        /// 所得人地址
        /// </summary>
        public String Address { get; set; }
        /// <summary>
        /// 國民身分證統一編號 or 所得單位統一編號(加密)
        /// </summary>
        public String IDNumberEnc { get; set; }
        /// <summary>
        /// 所得稅國家代碼
        /// </summary>
        public String TaxNationCode { get; set; }
        /// <summary>
        /// 外國人在華滿183天(滿：1、未滿：2)
        /// </summary>
        public String Check183 { get; set; }
    }


    /// <summary>
    /// 所屬年度扣繳金額
    /// </summary>
    public class IncomeTaxTransInfo
    {
        /// <summary>
        /// 所得編號
        /// </summary>
        public String AUT { get; set; }

        /// <summary>
        /// 所得所屬年度
        /// </summary>
        public String IncomeYearInterval { get; set; }

        /// <summary>
        /// 所得給付年度
        /// </summary>
        public String IncomeYear { get; set; }

        /// <summary>
        /// 給付總額
        /// </summary>
        public Double TaxableIncome { get; set; }

        /// <summary>
        /// 扣繳率
        /// </summary>
        public Double TaxRate { get; set; }

        /// <summary>
        /// 扣繳稅額
        /// </summary>
        public Double IncomeTax { get; set; }

        /// <summary>
        /// 給付淨額
        /// </summary>
        public Double PayAmount { get; set; }

        /// <summary>
        /// 依勞退條例自願提繳之退休金額
        /// </summary>
        public Double RetireAmount { get; set; }

    }
}
