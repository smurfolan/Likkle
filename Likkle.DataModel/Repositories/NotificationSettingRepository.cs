﻿using System;

namespace Likkle.DataModel.Repositories
{
    public class NotificationSettingRepository : INotificationSettingRepository
    {
        private ILikkleDbContext _dbContext;

        private bool _disposed = false;

        public NotificationSettingRepository(ILikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public Guid InsertNewSetting(AutomaticSubscriptionSetting setting)
        {
            this._dbContext.AutomaticSubscriptionSettings.Add(setting);
            this._dbContext.SaveChanges();

            return setting.Id;
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }
    }
}
