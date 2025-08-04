using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRPortal.WebAPI
{
    /// <summary>
    /// 查詢一般標準設定
    /// </summary>
    public class GeneralSettingsRes
    {
        /// <summary>
        /// 識別碼(Portal)
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// 代碼
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 英文名稱
        /// </summary>
        public string NameEN { get; set; }
    }
}