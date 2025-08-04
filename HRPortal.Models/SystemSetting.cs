using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Models
{
    /// <summary>
    /// 系統設定
    /// </summary>
    [Table("SystemSetting")]
    public class SystemSetting : BaseEntity
    {
        /// <summary>
        /// 設定項目
        /// </summary>
        [Required, StringLength(30)]
        [Index("SystemSetting_index", 1, IsUnique = true)]
        public string SettingKey { get; set; }
        /// <summary>
        /// 設定值
        /// </summary>
        [Required, StringLength(50)]
        public string SettingValue { get; set; }
    }
}
