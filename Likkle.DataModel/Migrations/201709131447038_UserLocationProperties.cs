namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserLocationProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Latitude", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Users", "Longitude", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Longitude");
            DropColumn("dbo.Users", "Latitude");
        }
    }
}
