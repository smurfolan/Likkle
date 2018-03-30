using System.Data.Entity;

namespace Likkle.DataModel
{
    public class LikkleDbContext : DbContext, ILikkleDbContext
    {
        public IDbSet<Area> Areas { get; set; }
        public IDbSet<Group> Groups { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Language> Languages { get; set; }
        public IDbSet<AutomaticSubscriptionSetting> AutomaticSubscriptionSettings { get; set; }
        public IDbSet<HistoryGroup> HistoryGroups { get; set; }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
