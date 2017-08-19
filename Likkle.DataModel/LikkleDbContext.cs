using System.Data.Entity;

namespace Likkle.DataModel
{
    // TODO: Currently, migrations are running right after a call is made to the API. In order to optimize this, try to run them before any call is made.
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
