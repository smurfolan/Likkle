using System.Data.Entity;

namespace Likkle.DataModel
{
    public interface ILikkleDbContext
    {
        IDbSet<Area> Areas { get; set; }
        IDbSet<Group> Groups { get; set; }
        IDbSet<Tag> Tags { get; set; }
        IDbSet<User> Users { get; set; }
        IDbSet<Language> Languages { get; set; }
        IDbSet<AutomaticSubscriptionSetting> AutomaticSubscriptionSettings { get; set; }
        IDbSet<HistoryGroup> HistoryGroups { get; set; }

        int SaveChanges();
    }
}
