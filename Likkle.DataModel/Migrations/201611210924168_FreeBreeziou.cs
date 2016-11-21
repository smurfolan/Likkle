namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FreeBreeziou : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationSettingTags",
                c => new
                    {
                        NotificationSetting_Id = c.Guid(nullable: false),
                        Tag_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.NotificationSetting_Id, t.Tag_Id })
                .ForeignKey("dbo.NotificationSettings", t => t.NotificationSetting_Id, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .Index(t => t.NotificationSetting_Id)
                .Index(t => t.Tag_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationSettingTags", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.NotificationSettingTags", "NotificationSetting_Id", "dbo.NotificationSettings");
            DropIndex("dbo.NotificationSettingTags", new[] { "Tag_Id" });
            DropIndex("dbo.NotificationSettingTags", new[] { "NotificationSetting_Id" });
            DropTable("dbo.NotificationSettingTags");
        }
    }
}
