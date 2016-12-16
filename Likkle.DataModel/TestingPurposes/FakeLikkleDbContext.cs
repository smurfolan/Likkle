using System;
using System.Data.Entity;

namespace Likkle.DataModel.TestingPurposes
{
    public class FakeLikkleDbContext : ILikkleDbContext
    {
        public FakeLikkleDbContext()
        {
            // this.Departments = new FakeDepartmentSet(); IF WE WANT TO HAVE CUSTOM .FIND IMPLEMENTATION
            this.Users = new FakeDbSet<User>();
            this.Areas = new FakeDbSet<Area>();
            this.Tags = new FakeDbSet<Tag>();
            this.Languages = new FakeDbSet<Language>();
            this.NotificationSettings = new FakeDbSet<NotificationSetting>();
            this.Groups = new FakeDbSet<Group>();
        }


        public IDbSet<Area> Areas { get; set; }
        public IDbSet<Group> Groups { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Language> Languages { get; set; }
        public IDbSet<NotificationSetting> NotificationSettings { get; set; }

        public int SaveChanges()
        {
            return 0;
        }

        public FakeLikkleDbContext Seed()
        {
            this.SeedTags();
            this.SeedLanguages();

            return this;
        }

        private void SeedTags()
        {
            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"),
                Name = "Help"
            });

            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8"),
                Name = "School"
            });

            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"),
                Name = "Sport"
            });

            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"),
                Name = "Sport"
            });

            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("ca97f757-f249-4d7d-ac80-814498348688"),
                Name = "University"
            });

            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("ffabb737-0fbd-4118-a23e-a28e5805caba"),
                Name = "Animals"
            });

            ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
            {
                Id = Guid.Parse("afc3c12f-b884-40e2-b356-2c863fd0b86c"),
                Name = "Other"
            });
        }

        private void SeedLanguages()
        {
            ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
            {
                Id = Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb"),
                Name = "Bulgarian"
            });

            ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
            {
                Id = Guid.Parse("05872235-365b-41f8-ab50-3913ffe9c601"),
                Name = "English"
            });

            ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
            {
                Id = Guid.Parse("4f168a6d-05f8-4da2-87d0-196b80dfcbe5"),
                Name = "Spanish"
            });

            ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
            {
                Id = Guid.Parse("6ebbd016-0686-4f71-8ebf-123d446054a2"),
                Name = "Russian"
            });

            ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
            {
                Id = Guid.Parse("79ae4cd5-9fcf-4140-acbe-c64a20298069"),
                Name = "German"
            });

            ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
            {
                Id = Guid.Parse("50057412-d211-4ebe-b7ae-a22520c9686c"),
                Name = "French"
            });
        }
    }
}
