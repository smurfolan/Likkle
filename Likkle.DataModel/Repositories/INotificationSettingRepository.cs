using System;

namespace Likkle.DataModel.Repositories
{
    public interface INotificationSettingRepository
    {
        Guid InsertNewSetting(NotificationSetting setting);
    }
}
