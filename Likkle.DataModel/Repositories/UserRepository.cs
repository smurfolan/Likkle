using System;
using System.Linq;

namespace Likkle.DataModel.Repositories
{
    public class UserRepository : IUserRepository
    {
        private LikkleDbContext _dbContext;

        private bool _disposed = false;

        public UserRepository(LikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public User GetUserById(Guid userId)
        {
            return this._dbContext.Users.FirstOrDefault(u => u.Id == userId);
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }
    }
}
