namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VisibleToThePublic_NewFieldForGroup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "VisibleToThePublic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "VisibleToThePublic");
        }
    }
}
