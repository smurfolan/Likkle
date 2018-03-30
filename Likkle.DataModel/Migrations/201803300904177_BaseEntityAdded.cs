namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BaseEntityAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Areas", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Groups", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Users", "DateCreated", c => c.DateTime(nullable: false));
            DropColumn("dbo.Groups", "VisibleToThePublic");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Groups", "VisibleToThePublic", c => c.Boolean(nullable: false));
            DropColumn("dbo.Users", "DateCreated");
            DropColumn("dbo.Groups", "DateCreated");
            DropColumn("dbo.Areas", "DateCreated");
        }
    }
}
