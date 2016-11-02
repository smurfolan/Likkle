using System;

namespace Likkle.DataModel.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<LikkleDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Likkle.DataModel.LikkleDbContext";
        }

        protected override void Seed(LikkleDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Languages.AddOrUpdate(
                l => l.Name, 
                new Language() { Id = Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb"), Name = "Bulgarian"},
                new Language() { Id = Guid.Parse("05872235-365b-41f8-ab50-3913ffe9c601"), Name = "English"},
                new Language() { Id = Guid.Parse("4f168a6d-05f8-4da2-87d0-196b80dfcbe5"),Name = "Spanish" },
                new Language() { Id = Guid.Parse("6ebbd016-0686-4f71-8ebf-123d446054a2"),Name = "Russian" },
                new Language() { Id = Guid.Parse("79ae4cd5-9fcf-4140-acbe-c64a20298069"),Name = "German" },
                new Language() { Id = Guid.Parse("50057412-d211-4ebe-b7ae-a22520c9686c"), Name = "French" });

            context.Tags.AddOrUpdate(
                t => t.Name, 
                new Tag() { Id = Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"),Name= "Help" },
                new Tag() { Id = Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8"),Name = "School" },
                new Tag() { Id = Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"),Name = "Sport" },
                new Tag() { Id = Guid.Parse("ca97f757-f249-4d7d-ac80-814498348688"),Name = "University" },
                new Tag() { Id = Guid.Parse("ffabb737-0fbd-4118-a23e-a28e5805caba"),Name = "Animals" },
                new Tag() { Id = Guid.Parse("afc3c12f-b884-40e2-b356-2c863fd0b86c"), Name = "Other" });
        }
    }
}
