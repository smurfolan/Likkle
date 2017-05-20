namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DunnoWhaat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "TwitterUsername", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "TwitterUsername");
        }
    }
}
