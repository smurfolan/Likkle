namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedRelation : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.NotificationSettings", "Id", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NotificationSettings", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("dbo.Users", "Id", c => c.Guid(nullable: false, identity: true));
        }
    }
}
