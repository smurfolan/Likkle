namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updated_NotificationSetting_Entity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tags", "NotificationSetting_Id", c => c.Guid());
            CreateIndex("dbo.Tags", "NotificationSetting_Id");
            AddForeignKey("dbo.Tags", "NotificationSetting_Id", "dbo.NotificationSettings", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tags", "NotificationSetting_Id", "dbo.NotificationSettings");
            DropIndex("dbo.Tags", new[] { "NotificationSetting_Id" });
            DropColumn("dbo.Tags", "NotificationSetting_Id");
        }
    }
}
