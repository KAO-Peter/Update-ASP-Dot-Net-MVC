using HRPortal.Core.Utilities;
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
    public abstract partial class BaseEntity
    {
        public BaseEntity()
        {
            ID = Utility.GenerateGuid();
            CreatedTime = DateTime.Now;
        }
        /// <summary>
        /// 識別編
        /// </summary>
        [Key, Column(Order = 0)]
        public Guid ID { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        [JsonIgnore]
        [Column(TypeName = "DateTime2")]
        public DateTime CreatedTime { get; set; }
    }
}
