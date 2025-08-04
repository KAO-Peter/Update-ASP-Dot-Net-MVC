//using HRPortal.Models.Attributes;
using HRPortal.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Models
{
    /// <summary>
    /// 角色選單對應
    /// </summary>
    [Table("RoleMenuMaps")]
    public class RoleMenuMap : BaseEntity
    {
        /// <summary>
        /// 角色識別碼
        /// </summary>
        [ForeignKey("Role")]
        public Guid Role_ID { get; set; }
        /// <summary>
        /// 選單識別碼
        /// </summary>
        [ForeignKey("Menu")]
        public Guid Menu_ID { get; set; }
        /// <summary>
        /// 選單參數
        /// </summary>
        public string MenuParams { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        [JsonIgnore]
        public virtual Role Role { get; set; }
        /// <summary>
        /// 選單
        /// </summary>
        [JsonIgnore]
        public virtual Menu Menu { get; set; }
    }
}
