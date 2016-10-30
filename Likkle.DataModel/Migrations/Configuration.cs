using System;

namespace Likkle.DataModel.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Likkle.DataModel.LikkleDbContext>
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
                new Language() { Id = Guid.NewGuid(), Name = "Bugarian"},
                new Language() { Id = Guid.NewGuid(), Name = "English"},
                new Language() { Id = Guid.NewGuid(),Name = "Spanish" },
                new Language() { Id = Guid.NewGuid(),Name = "Russian" },
                new Language() { Id = Guid.NewGuid(),Name = "German" },
                new Language() { Id = Guid.NewGuid(), Name = "French" });

            context.Tags.AddOrUpdate(
                t => t.Name, 
                new Tag() { Id = Guid.NewGuid(),Name= "Help" },
                new Tag() { Id = Guid.NewGuid(),Name = "School" },
                new Tag() { Id = Guid.NewGuid(),Name = "Sport" },
                new Tag() { Id = Guid.NewGuid(),Name = "University" },
                new Tag() { Id = Guid.NewGuid(),Name = "Animals" },
                new Tag() { Id = Guid.NewGuid(), Name = "Other" });
        }
    }
}
