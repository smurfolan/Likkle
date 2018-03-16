using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Likkle.DataModel.TestingPurposes
{
    public class FakeLikkleDbContext : ILikkleDbContext
    {
        private static Dictionary<Guid, string> AvailableTags = new Dictionary<Guid, string>()
        {
            { Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"), "Help"},
            { Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8"), "School" },
            { Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"), "Sport"},
            { Guid.Parse("ca97f757-f249-4d7d-ac80-814498348688"), "University"},
            { Guid.Parse("ffabb737-0fbd-4118-a23e-a28e5805caba"), "Animals"},
            { Guid.Parse("afc3c12f-b884-40e2-b356-2c863fd0b86c"), "Other"}
        };

        private static Dictionary<Guid, string> AvailableLanguages = new Dictionary<Guid, string>()
        {
            { Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb"), "Bulgarian"},
            { Guid.Parse("05872235-365b-41f8-ab50-3913ffe9c601"), "English"},
            { Guid.Parse("4f168a6d-05f8-4da2-87d0-196b80dfcbe5"), "Spanish"},
            { Guid.Parse("6ebbd016-0686-4f71-8ebf-123d446054a2"), "Russian"},
            { Guid.Parse("79ae4cd5-9fcf-4140-acbe-c64a20298069"), "German"},
            { Guid.Parse("50057412-d211-4ebe-b7ae-a22520c9686c"), "French"}
        };

        public FakeLikkleDbContext()
        {
            // this.Departments = new FakeDepartmentSet(); IF WE WANT TO HAVE CUSTOM .FIND IMPLEMENTATION
            this.Users = new FakeDbSet<User>();
            this.Areas = new FakeDbSet<Area>();
            this.Tags = new FakeDbSet<Tag>();
            this.Languages = new FakeDbSet<Language>();
            this.AutomaticSubscriptionSettings = new FakeDbSet<AutomaticSubscriptionSetting>();
            this.Groups = new FakeDbSet<Group>();
            this.HistoryGroups = new FakeDbSet<HistoryGroup>();
        }


        public IDbSet<Area> Areas { get; set; }
        public IDbSet<Group> Groups { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Language> Languages { get; set; }
        public IDbSet<AutomaticSubscriptionSetting> AutomaticSubscriptionSettings { get; set; }
        public IDbSet<HistoryGroup> HistoryGroups { get; set; }

        public int SaveChanges()
        {
            return 0;
        }

        public FakeLikkleDbContext Seed()
        {
            SeedTags();
            SeedLanguages();

            return this;
        }

        public static Dictionary<Guid, string> GetAllAvailableTags() => AvailableTags;
        public static Dictionary<Guid, string> GetAllAvailableLanguages() => AvailableLanguages;

        private void SeedTags()
        {
            foreach (var predefinedtag in AvailableTags)
            {
                ((FakeDbSet<Tag>)Tags).AddPredefined(new Tag()
                {
                    Id = predefinedtag.Key,
                    Name = predefinedtag.Value
                });
            }
        }

        private void SeedLanguages()
        {
            foreach (var predefinedLanguage in AvailableLanguages)
            {
                ((FakeDbSet<Language>)Languages).AddPredefined(new Language()
                {
                    Id = predefinedLanguage.Key,
                    Name = predefinedLanguage.Value
                });
            }
        }
    }
}
