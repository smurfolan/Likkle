using System;
using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public interface IUserRepository
    {
        User GetUserById(Guid userId);
        User GetUserByStsId(string stsId);
        IEnumerable<User> GetAllUsers();
        Guid InsertNewUser(User newUser);
    }
}
