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
    }
}
