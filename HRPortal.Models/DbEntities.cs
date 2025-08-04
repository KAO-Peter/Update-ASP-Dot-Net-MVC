using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Models
{
    public class DbEntities : DbContext
    {
        public DbEntities()
            : base("name=DefaultConnection")
        {

        }

        public DbSet<Company> Companys { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleMenuMap> RoleMenuMaps { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
    }
}
