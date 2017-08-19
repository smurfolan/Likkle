namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotifiationSettingsRenamed : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TagNotificationSettings", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.TagNotificationSettings", "NotificationSetting_Id", "dbo.NotificationSettings");
            DropForeignKey("dbo.NotificationSettings", "Id", "dbo.Users");
            DropIndex("dbo.TagNotificationSettings", new[] { "Tag_Id" });
            DropIndex("dbo.TagNotificationSettings", new[] { "NotificationSetting_Id" });
            DropIndex("dbo.NotificationSettings", new[] { "Id" });
            CreateTable(
                "dbo.AutomaticSubscriptionSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AutomaticallySubscribeToAllGroups = c.Boolean(nullable: false),
                        AutomaticallySubscribeToAllGroupsWithTag = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.TagAutomaticSubscriptionSettings",
                c => new
                    {
                        Tag_Id = c.Guid(nullable: false),
                        AutomaticSubscriptionSetting_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.AutomaticSubscriptionSetting_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.AutomaticSubscriptionSettings", t => t.AutomaticSubscriptionSetting_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.AutomaticSubscriptionSetting_Id);

            DropTable("dbo.TagNotificationSettings");
            DropTable("dbo.NotificationSettings");          
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TagNotificationSettings",
                c => new
                    {
                        Tag_Id = c.Guid(nullable: false),
                        NotificationSetting_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.NotificationSetting_Id });
            
            CreateTable(
                "dbo.NotificationSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AutomaticallySubscribeToAllGroups = c.Boolean(nullable: false),
                        AutomaticallySubscribeToAllGroupsWithTag = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.AutomaticSubscriptionSettings", "Id", "dbo.Users");
            DropForeignKey("dbo.TagAutomaticSubscriptionSettings", "AutomaticSubscriptionSetting_Id", "dbo.AutomaticSubscriptionSettings");
            DropForeignKey("dbo.TagAutomaticSubscriptionSettings", "Tag_Id", "dbo.Tags");
            DropIndex("dbo.AutomaticSubscriptionSettings", new[] { "Id" });
            DropIndex("dbo.TagAutomaticSubscriptionSettings", new[] { "AutomaticSubscriptionSetting_Id" });
            DropIndex("dbo.TagAutomaticSubscriptionSettings", new[] { "Tag_Id" });
            DropTable("dbo.TagAutomaticSubscriptionSettings");
            DropTable("dbo.AutomaticSubscriptionSettings");
            CreateIndex("dbo.NotificationSettings", "Id");
            CreateIndex("dbo.TagNotificationSettings", "NotificationSetting_Id");
            CreateIndex("dbo.TagNotificationSettings", "Tag_Id");
            AddForeignKey("dbo.NotificationSettings", "Id", "dbo.Users", "Id");
            AddForeignKey("dbo.TagNotificationSettings", "NotificationSetting_Id", "dbo.NotificationSettings", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TagNotificationSettings", "Tag_Id", "dbo.Tags", "Id", cascadeDelete: true);
        }
    }
}
