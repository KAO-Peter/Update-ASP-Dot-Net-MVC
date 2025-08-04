using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class ShowAttachmentContentViewModel
    {
        public Guid AttachmentGUID { get; set; }
        public string AttachmentFilePath { get; set; }
        public string ContentTitle { get; set; }

    }
}
