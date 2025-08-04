using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Models.Interfaces
{
    public interface IMenu : IDisable
    {
        Guid ID { get; set; }

        Guid? Parent_ID { get; set; }

        string Title { get; set; }

        string Alias { get; set; }

        string Link { get; set; }

        int Type { get; set; }

        int Ordering { get; set; }

        string Description { get; set; }
    }
}
