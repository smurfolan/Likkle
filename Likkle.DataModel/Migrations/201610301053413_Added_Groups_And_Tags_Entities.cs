namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Groups_And_Tags_Entities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TagGroups",
                c => new
                    {
                        Tag_Id = c.Guid(nullable: false),
                        Group_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.Group_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.Group_Id);
            
            AddColumn("dbo.Areas", "Radius", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TagGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.TagGroups", "Tag_Id", "dbo.Tags");
            DropIndex("dbo.TagGroups", new[] { "Group_Id" });
            DropIndex("dbo.TagGroups", new[] { "Tag_Id" });
            DropColumn("dbo.Areas", "Radius");
            DropTable("dbo.TagGroups");
            DropTable("dbo.Tags");
        }
    }
}
