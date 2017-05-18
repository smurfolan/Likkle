namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StatusPropertyAddedToGroupsAndAreas : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Areas", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "IsActive");
            DropColumn("dbo.Areas", "IsActive");
        }
    }
}
