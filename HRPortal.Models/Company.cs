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
    /// 公司
    /// </summary>
    [Table("Companys")]
    public class Company : BaseEntity
    {
        /// <summary>
        /// 統一編號
        /// </summary>
        [StringLength(8)]
        public string TaxID { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        [Required, StringLength(64)]
        public string Name { get; set; }
        /// <summary>
        /// 別名
        /// </summary>
        [Required, StringLength(16)]
        [Index("Companys_Alias", IsUnique = true)]
        public string Alias { get; set; }

        /// <summary>
        /// 所有員工
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Employee> Employees { get; set; }
        /// <summary>
        /// 所有部門
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<Department> Departments { get; set; }
    }
}
