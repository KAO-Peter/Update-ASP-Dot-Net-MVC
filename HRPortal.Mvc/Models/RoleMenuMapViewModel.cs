using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class RoleMenuMapViewModel
    {
        public List<MenuGroupItem> RoleMenus { get; set; }

        public RoleMenuMapViewModel()
        {
            RoleMenus = new List<MenuGroupItem>();
        }
    }

    public class MenuGroupItem
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public List<RoleMenuItem> Items { get; set; }

        public MenuGroupItem()
        {
            Items = new List<RoleMenuItem>();
        }
    }

    public class RoleMenuItem
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
    }
}
