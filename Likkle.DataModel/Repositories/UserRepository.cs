using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public User GetUserByStsId(string stsId)
        {
            return this._dbContext.Users.FirstOrDefault(u => u.IdsrvUniqueId == stsId);
        }


        public IEnumerable<User> GetAllUsers()
        {
            return this._dbContext.Users.Include(u => u.NotificationSettings);
        }

        public Guid InsertNewUser(User newUser)
        {
            this._dbContext.Users.Add(newUser);
            this._dbContext.SaveChanges();

            return newUser.Id;
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }
    }
}
