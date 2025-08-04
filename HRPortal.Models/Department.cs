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
    /// 部門
    /// </summary>
    [Table("Departments")]
    public class Department : BaseEntity
    {
        /// <summary>
        /// 公司識別碼
        /// </summary>
        [ForeignKey("Company")]
        [Index("Departments_index", 1, IsUnique = true)]
        public Guid Company_ID { get; set; }
        /// <summary>
        /// 部門代碼
        /// </summary>
        [Index("Departments_index", 2, IsUnique = true)]
        [Required, StringLength(12)]
        public string DepartmentNO { get; set; }
        /// <summary>
        /// 部門名稱
        /// </summary>
        [Required, StringLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        [JsonIgnore]
        public virtual Company Company { get; set; }
    }
}
