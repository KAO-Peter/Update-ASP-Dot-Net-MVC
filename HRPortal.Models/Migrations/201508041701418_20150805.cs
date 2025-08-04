namespace HRPortal.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20150805 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Departments", name: "CompanyID", newName: "Company_ID");
            RenameColumn(table: "dbo.Employees", name: "CompanyID", newName: "Company_ID");
            RenameColumn(table: "dbo.Employees", name: "DepartmentID", newName: "Department_ID");
            RenameIndex(table: "dbo.Employees", name: "IX_DepartmentID", newName: "IX_Department_ID");
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 32),
                        RoleParams = c.String(),
                        Description = c.String(maxLength: 512),
                        CreatedTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.RoleMenuMaps",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Role_ID = c.Guid(nullable: false),
                        Menu_ID = c.Guid(nullable: false),
                        MenuParams = c.String(),
                        CreatedTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Menus", t => t.Menu_ID, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.Role_ID, cascadeDelete: true)
                .Index(t => t.Role_ID)
                .Index(t => t.Menu_ID);
            
            CreateTable(
                "dbo.Menus",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Title = c.String(maxLength: 128),
                        Alias = c.String(maxLength: 32),
                        Link = c.String(maxLength: 128),
                        Type = c.Int(nullable: false),
                        Ordering = c.Int(nullable: false),
                        Parent_ID = c.Guid(),
                        Description = c.String(maxLength: 512),
                        DisableDate = c.DateTime(),
                        CreatedTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Menus", t => t.Parent_ID)
                .Index(t => t.Parent_ID);
            
            AddColumn("dbo.Employees", "TelAreaCode", c => c.String(maxLength: 8));
            AddColumn("dbo.Employees", "TelExt", c => c.String(maxLength: 8));
            AddColumn("dbo.Employees", "PasswordHash", c => c.String(maxLength: 256));
            AddColumn("dbo.Employees", "Role_ID", c => c.Guid(nullable: false));
            AddColumn("dbo.Employees", "ReportTo_ID", c => c.Guid());
            AddColumn("dbo.Employees", "TopExecutive", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Employees", "Role_ID");
            CreateIndex("dbo.Employees", "ReportTo_ID");
            AddForeignKey("dbo.Employees", "ReportTo_ID", "dbo.Employees", "ID");
            AddForeignKey("dbo.Employees", "Role_ID", "dbo.Roles", "ID", cascadeDelete: true);
            DropColumn("dbo.Employees", "Tel_Area_Code");
            DropColumn("dbo.Employees", "Tel_Ext");
            DropColumn("dbo.Employees", "Password_Hash");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "Password_Hash", c => c.String(maxLength: 256));
            AddColumn("dbo.Employees", "Tel_Ext", c => c.String(maxLength: 8));
            AddColumn("dbo.Employees", "Tel_Area_Code", c => c.String(maxLength: 8));
            DropForeignKey("dbo.Employees", "Role_ID", "dbo.Roles");
            DropForeignKey("dbo.RoleMenuMaps", "Role_ID", "dbo.Roles");
            DropForeignKey("dbo.RoleMenuMaps", "Menu_ID", "dbo.Menus");
            DropForeignKey("dbo.Menus", "Parent_ID", "dbo.Menus");
            DropForeignKey("dbo.Employees", "ReportTo_ID", "dbo.Employees");
            DropIndex("dbo.Menus", new[] { "Parent_ID" });
            DropIndex("dbo.RoleMenuMaps", new[] { "Menu_ID" });
            DropIndex("dbo.RoleMenuMaps", new[] { "Role_ID" });
            DropIndex("dbo.Employees", new[] { "ReportTo_ID" });
            DropIndex("dbo.Employees", new[] { "Role_ID" });
            DropColumn("dbo.Employees", "TopExecutive");
            DropColumn("dbo.Employees", "ReportTo_ID");
            DropColumn("dbo.Employees", "Role_ID");
            DropColumn("dbo.Employees", "PasswordHash");
            DropColumn("dbo.Employees", "TelExt");
            DropColumn("dbo.Employees", "TelAreaCode");
            DropTable("dbo.Menus");
            DropTable("dbo.RoleMenuMaps");
            DropTable("dbo.Roles");
            RenameIndex(table: "dbo.Employees", name: "IX_Department_ID", newName: "IX_DepartmentID");
            RenameColumn(table: "dbo.Employees", name: "Department_ID", newName: "DepartmentID");
            RenameColumn(table: "dbo.Employees", name: "Company_ID", newName: "CompanyID");
            RenameColumn(table: "dbo.Departments", name: "Company_ID", newName: "CompanyID");
        }
    }
}
