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
    /// 選單
    /// </summary>
    [Table("Menus")]
    public class Menu : BaseEntity, IMenu
    {
        public Menu()
        {
            this.Children = new HashSet<Menu>();
            this.Roles = new HashSet<RoleMenuMap>();
        }

        /// <summary>
        /// 標題
        /// </summary>
        [StringLength(128)]
        public string Title { get; set; }
        /// <summary>
        /// 別名
        /// </summary>
        [StringLength(32)]
        public string Alias { get; set; }
        /// <summary>
        /// 連結
        /// </summary>
        [StringLength(128)]
        public string Link { get; set; }
        /// <summary>
        /// 類型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Ordering { get; set; }
        /// <summary>
        /// 上層選單識別碼
        /// </summary>
        [ForeignKey("Parent")]
        public Guid? Parent_ID { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(512)]
        public string Description { get; set; }
        /// <summary>
        /// 停用日期
        /// </summary>
        public DateTime? DisableDate { get; set; }
        /// <summary>
        /// 上層選單
        /// </summary>
        [JsonIgnore]
        public virtual Menu Parent { get; set; }
        /// <summary>
        /// 下層所有選單
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Menu> Children { get; set; }
        /// <summary>
        /// 所有角色
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<RoleMenuMap> Roles { get; set; }
    }

    public enum MenuGroup : int
    {
        /// <summary>
        /// 後台
        /// </summary>
        BACK_END = 1,
        /// <summary>
        /// 前台
        /// </summary>
        FRONT_END = 2
    }
    public enum MenuType : int
    {
        /// <summary>
        /// 根
        /// </summary
        ROOT = 0,
        /// <summary>
        /// 選單
        /// </summary>
        MENU = 1,
        /// <summary>
        /// 標籤
        /// </summary>
        TAB = 2
    }
}
