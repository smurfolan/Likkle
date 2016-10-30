namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GroupAreas",
                c => new
                    {
                        Group_Id = c.Guid(nullable: false),
                        Area_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Group_Id, t.Area_Id })
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: true)
                .ForeignKey("dbo.Areas", t => t.Area_Id, cascadeDelete: true)
                .Index(t => t.Group_Id)
                .Index(t => t.Area_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupAreas", "Area_Id", "dbo.Areas");
            DropForeignKey("dbo.GroupAreas", "Group_Id", "dbo.Groups");
            DropIndex("dbo.GroupAreas", new[] { "Area_Id" });
            DropIndex("dbo.GroupAreas", new[] { "Group_Id" });
            DropTable("dbo.GroupAreas");
            DropTable("dbo.Groups");
        }
    }
}
