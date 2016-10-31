namespace Likkle.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserLanguages", newName: "LanguageUsers");
            RenameTable(name: "dbo.Groups", newName: "UserGroups");
            DropForeignKey("dbo.Groups", "User_Id", "dbo.Users");
            DropIndex("dbo.Groups", new[] { "User_Id" });
            CreateIndex("dbo.UserGroups", "User_Id");
            CreateIndex("dbo.UserGroups", "Group_Id");
            AddForeignKey("dbo.UserGroups", "User_Id", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserGroups", "Group_Id", "dbo.Groups", "Id", cascadeDelete: true);
            DropColumn("dbo.Groups", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Groups", "User_Id", c => c.Guid());
            DropForeignKey("dbo.UserGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.UserGroups", "User_Id", "dbo.Users");
            DropIndex("dbo.UserGroups", new[] { "Group_Id" });
            DropIndex("dbo.UserGroups", new[] { "User_Id" });
            CreateIndex("dbo.Groups", "User_Id");
            AddForeignKey("dbo.Groups", "User_Id", "dbo.Users", "Id");
            RenameTable(name: "dbo.UserGroups", newName: "Groups");
            RenameTable(name: "dbo.LanguageUsers", newName: "UserLanguages");
        }
    }
}
