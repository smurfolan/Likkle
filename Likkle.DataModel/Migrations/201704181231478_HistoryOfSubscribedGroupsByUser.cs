namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HistoryOfSubscribedGroupsByUser : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.NotificationSettingTags", newName: "TagNotificationSettings");
            CreateTable(
                "dbo.HistoryGroups",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        DateTimeGroupWasSubscribed = c.DateTime(nullable: false),
                        UserId = c.Guid(nullable: false),
                        GroupId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HistoryGroups", "UserId", "dbo.Users");
            DropForeignKey("dbo.HistoryGroups", "GroupId", "dbo.Groups");
            DropIndex("dbo.HistoryGroups", new[] { "UserId" });
            DropIndex("dbo.HistoryGroups", new[] { "GroupId" });
            DropTable("dbo.HistoryGroups");
            RenameTable(name: "dbo.TagNotificationSettings", newName: "NotificationSettingTags");
        }
    }
}
