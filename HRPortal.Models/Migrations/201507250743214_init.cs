namespace HRPortal.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companys",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        TaxID = c.String(maxLength: 8),
                        Name = c.String(nullable: false, maxLength: 64),
                        Alias = c.String(nullable: false, maxLength: 16),
                        CreatedTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.Alias, unique: true, name: "Companys_Alias");
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        CompanyID = c.Guid(nullable: false),
                        DepartmentNO = c.String(nullable: false, maxLength: 12),
                        Name = c.String(nullable: false, maxLength: 64),
                        CreatedTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Companys", t => t.CompanyID, cascadeDelete: true)
                .Index(t => new { t.CompanyID, t.DepartmentNO }, unique: true, name: "Departments_index");
            
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        EmployeeNO = c.String(nullable: false, maxLength: 16),
                        CompanyID = c.Guid(nullable: false),
                        DepartmentID = c.Guid(),
                        Name = c.String(nullable: false, maxLength: 32),
                        Gender = c.String(nullable: false, maxLength: 8),
                        Cel = c.String(maxLength: 16),
                        Tel_Area_Code = c.String(maxLength: 8),
                        Tel = c.String(maxLength: 16),
                        Tel_Ext = c.String(maxLength: 8),
                        Email = c.String(maxLength: 256),
                        Password_Hash = c.String(maxLength: 256),
                        ArriveDate = c.DateTime(nullable: false),
                        LeaveDate = c.DateTime(),
                        DisableDate = c.DateTime(),
                        Description = c.String(maxLength: 512),
                        CreatedTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Companys", t => t.CompanyID, cascadeDelete: true)
                .ForeignKey("dbo.Departments", t => t.DepartmentID)
                .Index(t => new { t.EmployeeNO, t.CompanyID }, unique: true, name: "Employees_index")
                .Index(t => t.DepartmentID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Employees", "DepartmentID", "dbo.Departments");
            DropForeignKey("dbo.Employees", "CompanyID", "dbo.Companys");
            DropForeignKey("dbo.Departments", "CompanyID", "dbo.Companys");
            DropIndex("dbo.Employees", new[] { "DepartmentID" });
            DropIndex("dbo.Employees", "Employees_index");
            DropIndex("dbo.Departments", "Departments_index");
            DropIndex("dbo.Companys", "Companys_Alias");
            DropTable("dbo.Employees");
            DropTable("dbo.Departments");
            DropTable("dbo.Companys");
        }
    }
}
