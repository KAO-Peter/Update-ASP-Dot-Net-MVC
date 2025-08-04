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
    /// 員工
    /// </summary>
    [Table("Employees")]
    public class Employee : BaseEntity
    {
        /// <summary>
        /// 工號
        /// </summary>
        [Required, StringLength(16)]
        [Index("Employees_index", 1, IsUnique = true)]
        public string EmployeeNO { get; set; }
        /// <summary>
        /// 公司識別碼
        /// </summary>
        [ForeignKey("Company")]
        [Index("Employees_index", 2, IsUnique = true)]
        public Guid Company_ID { get; set; }
        /// <summary>
        /// 部門識別碼
        /// </summary>
        [ForeignKey("Department")]
        public Guid? Department_ID { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [Required]
        [StringLength(32)]
        public string Name { get; set; }
        /// <summary>
        /// 性別
        /// </summary>
        [Required]
        [StringLength(8)]
        public string Gender { get; set; }
        /// <summary>
        /// 行動電話
        /// </summary>
        [StringLength(16)]
        public string Cel { get; set; }
        /// <summary>
        /// 聯絡電話區碼
        /// </summary>
        [StringLength(8)]
        public string TelAreaCode { get; set; }
        /// <summary>
        /// 聯絡電話
        /// </summary>
        [StringLength(16)]
        public string Tel { get; set; }
        /// <summary>
        /// 聯絡電話分機號碼
        /// </summary>
        [StringLength(8)]
        public string TelExt { get; set; }
        /// <summary>
        /// 電子郵件
        /// </summary>
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }
        /// <summary>
        /// 密碼雜湊
        /// </summary>
        [StringLength(256)]
        public string PasswordHash { get; set; }
        /// <summary>
        /// 到職日期
        /// </summary>
        [Required]
        public DateTime ArriveDate { get; set; }
        /// <summary>
        /// 角色識別碼
        /// </summary>
        [ForeignKey("Role")]
        public Guid Role_ID { get; set; }
        /// <summary>
        /// 離職日期
        /// </summary>
        public DateTime? LeaveDate { get; set; }
        /// <summary>
        /// 停用日期
        /// </summary>
        public DateTime? DisableDate { get; set; }
        /// <summary>
        /// 主管識別碼
        /// </summary>
        [ForeignKey("ReportTo")]
        public Guid? ReportTo_ID { get; set; }
        /// <summary>
        /// 是否為最高主管
        /// </summary>
        public bool TopExecutive { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(512)]
        public string Description { get; set; }
        /// <summary>
        /// 公司
        /// </summary>
        [JsonIgnore]
        public virtual Company Company { get; set; }
        /// <summary>
        /// 部門
        /// </summary>
        [JsonIgnore]
        public virtual Department Department { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        [JsonIgnore]
        public virtual Role Role { get; set; }
        /// <summary>
        /// 簽核主管
        /// </summary>
        [JsonIgnore]
        public virtual Employee ReportTo { get; set; }
    }
}
