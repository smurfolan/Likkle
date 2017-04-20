namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSocialLinksToUserProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "InstagramUsername", c => c.String());
            AddColumn("dbo.Users", "FacebookUsername", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "FacebookUsername");
            DropColumn("dbo.Users", "InstagramUsername");
        }
    }
}
