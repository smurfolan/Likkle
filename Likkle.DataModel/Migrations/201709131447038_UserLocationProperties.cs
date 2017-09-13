namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserLocationProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Latitude", c => c.Double(nullable: false));
            AddColumn("dbo.Users", "Longitude", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Longitude");
            DropColumn("dbo.Users", "Latitude");
        }
    }
}
