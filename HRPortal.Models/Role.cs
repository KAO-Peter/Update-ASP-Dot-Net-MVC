//using HRPortal.Models.Attributes;
using HRPortal.Models.Interfaces;
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
    /// 角色
    /// </summary>
    [Table("Roles")]
    public class Role : BaseEntity
    {
        public Role()
        {
            this.Menus = new HashSet<RoleMenuMap>();
        }
        /// <summary>
        /// 名稱
        /// </summary>
        [Required, StringLength(32)]
        public string Name { get; set; }
        /// <summary>
        /// 選單參數
        /// </summary>
        public string RoleParams { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(512)]
        public string Description { get; set; }
        /// <summary>
        /// 所有選單
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<RoleMenuMap> Menus { get; set; }
    }
}
