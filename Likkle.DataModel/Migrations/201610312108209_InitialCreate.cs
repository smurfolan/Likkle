namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Areas",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        Radius = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        IdsrvUniqueId = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        About = c.String(),
                        Gender = c.Int(nullable: false),
                        BirthDate = c.DateTime(nullable: false),
                        PhoneNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Languages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NotificationSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        AutomaticallySubscribeToAllGroups = c.Boolean(nullable: false),
                        AutomaticallySubscribeToAllGroupsWithTag = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Id)
                .Index(t => t.Id);
            
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
            
            CreateTable(
                "dbo.UserGroups",
                c => new
                    {
                        User_Id = c.Guid(nullable: false),
                        Group_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Group_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.LanguageUsers",
                c => new
                    {
                        Language_Id = c.Guid(nullable: false),
                        User_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Language_Id, t.User_Id })
                .ForeignKey("dbo.Languages", t => t.Language_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Language_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationSettings", "Id", "dbo.Users");
            DropForeignKey("dbo.LanguageUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.LanguageUsers", "Language_Id", "dbo.Languages");
            DropForeignKey("dbo.UserGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.UserGroups", "User_Id", "dbo.Users");
            DropForeignKey("dbo.TagGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.TagGroups", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.GroupAreas", "Area_Id", "dbo.Areas");
            DropForeignKey("dbo.GroupAreas", "Group_Id", "dbo.Groups");
            DropIndex("dbo.NotificationSettings", new[] { "Id" });
            DropIndex("dbo.LanguageUsers", new[] { "User_Id" });
            DropIndex("dbo.LanguageUsers", new[] { "Language_Id" });
            DropIndex("dbo.UserGroups", new[] { "Group_Id" });
            DropIndex("dbo.UserGroups", new[] { "User_Id" });
            DropIndex("dbo.TagGroups", new[] { "Group_Id" });
            DropIndex("dbo.TagGroups", new[] { "Tag_Id" });
            DropIndex("dbo.GroupAreas", new[] { "Area_Id" });
            DropIndex("dbo.GroupAreas", new[] { "Group_Id" });
            DropTable("dbo.LanguageUsers");
            DropTable("dbo.UserGroups");
            DropTable("dbo.TagGroups");
            DropTable("dbo.GroupAreas");
            DropTable("dbo.NotificationSettings");
            DropTable("dbo.Languages");
            DropTable("dbo.Users");
            DropTable("dbo.Tags");
            DropTable("dbo.Groups");
            DropTable("dbo.Areas");
        }
    }
}
