namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class User_Entity_Added_And_Automatic_Db_Seed : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Languages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
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
                "dbo.UserLanguages",
                c => new
                    {
                        User_Id = c.Guid(nullable: false),
                        Language_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Language_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Languages", t => t.Language_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Language_Id);
            
            AddColumn("dbo.Groups", "User_Id", c => c.Guid());
            CreateIndex("dbo.Groups", "User_Id");
            AddForeignKey("dbo.Groups", "User_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationSettings", "Id", "dbo.Users");
            DropForeignKey("dbo.UserLanguages", "Language_Id", "dbo.Languages");
            DropForeignKey("dbo.UserLanguages", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Groups", "User_Id", "dbo.Users");
            DropIndex("dbo.NotificationSettings", new[] { "Id" });
            DropIndex("dbo.UserLanguages", new[] { "Language_Id" });
            DropIndex("dbo.UserLanguages", new[] { "User_Id" });
            DropIndex("dbo.Groups", new[] { "User_Id" });
            DropColumn("dbo.Groups", "User_Id");
            DropTable("dbo.UserLanguages");
            DropTable("dbo.NotificationSettings");
            DropTable("dbo.Users");
            DropTable("dbo.Languages");
        }
    }
}
