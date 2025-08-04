namespace HRPortal.Models.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<HRPortal.Models.DbEntities>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(HRPortal.Models.DbEntities context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            Guid company_id = new Guid("00000001-0001-0000-0000-000000000000");

            context.Companys.AddOrUpdate(
              x => x.ID,
              new Company
              {
                  ID = company_id,
                  Name = "鼎鼎企管",
                  Alias = "DDMC"
              }
            );

            context.Departments.AddOrUpdate(
              x => x.ID,
              new Department
              {
                  ID=new Guid("00000002-0001-0000-0000-000000000000"),
                  Company_ID = company_id,
                  Name = "測試一課",
                  DepartmentNO = "Test",
              }
            );

            context.Employees.AddOrUpdate(
              x => x.ID,
              new Employee
              {
                  ID = new Guid("00000013-0001-0001-0000-000000000000"),
                  Name = "管理員",
                  EmployeeNO = "admin",
                  Company_ID = company_id,
                  Department_ID = new Guid("00000002-0001-0000-0000-000000000000"),
                  Role_ID = new Guid("00000019-0001-0000-0000-000000000000"),
                  Gender = "1",
                  Email = "derick+1@ya.com.tw",
                  ArriveDate = DateTime.Now.Date,
              }
            );

            context.Roles.AddOrUpdate(x => x.ID,
                new Role
                {
                    ID = new Guid("00000019-0001-0000-0000-000000000000"),
                    Name = "管理員",
                    RoleParams = "{\"is_admin\":true}",
                },
                new Role
                {
                    ID = new Guid("00000019-0002-0000-0000-000000000000"),
                    Name = "窗口",
                },
                new Role
                {
                    ID = new Guid("00000019-0003-0000-0000-000000000000"),
                    Name = "一般使用者",
                }                
            );
        }
    }
}
