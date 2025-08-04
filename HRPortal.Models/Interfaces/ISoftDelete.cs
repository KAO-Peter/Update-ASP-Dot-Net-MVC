using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Models.Interfaces
{
    public interface ISoftDelete
    {
        DateTime? deleted_time { get; set; }
    }
}
