using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class RoleMenuMapsService : BaseCrudService<RoleMenuMap>
    {
        public RoleMenuMapsService(HRPortal_Services services)
            : base(services)
        {
        }

        public IQueryable<RoleMenuMap> GetByRole(Guid roleId)
        {
            return Where(x => x.Role_ID == roleId);
        }

        public void SetRoleMenus(Guid roleId, List<Guid> menuList)
        {
            Delete(Where(x => x.Role_ID == roleId));
            foreach (Guid menuId in menuList)
            {
                RoleMenuMap menuItem = new RoleMenuMap();
                menuItem.ID = Guid.NewGuid();
                menuItem.Role_ID = roleId;
                menuItem.Menu_ID = menuId;
                menuItem.CreatedTime = DateTime.Now;

                Create(menuItem);
            }
        }
    }
}
