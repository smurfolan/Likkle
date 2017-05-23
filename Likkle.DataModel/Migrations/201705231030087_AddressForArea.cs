namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddressForArea : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Areas", "ApproximateAddress", c => c.String());
            AddColumn("dbo.Users", "TwitterUsername", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "TwitterUsername");
            DropColumn("dbo.Areas", "ApproximateAddress");
        }
    }
}
