using System;

namespace Likkle.DataModel.Repositories
{
    public interface IUserRepository
    {
        User GetUserById(Guid userId);
    }
}
