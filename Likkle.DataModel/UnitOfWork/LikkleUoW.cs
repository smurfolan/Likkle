﻿using Likkle.DataModel.Repositories;

namespace Likkle.DataModel.UnitOfWork
{
    public class LikkleUoW : ILikkleUoW
    {
        // TODO: Make this injectable
        private readonly ILikkleDbContext _dbContext;

        public LikkleUoW(ILikkleDbContext context)
        {
            _dbContext = context;
        }

        private AreaRepository areaRepository;
        private GroupRepository groupRepository;
        private TagRepository tagRepository;
        private UserRepository userRepository;
        private LanguageRepository languageRepository;
        private NotificationSettingRepository notificationSettingRepository;
        private HistoryGroupRepository historyGroupRepository;

        public AreaRepository AreaRepository
        {
            get
            {
                if (areaRepository == null)
                    return new AreaRepository(_dbContext);

                return areaRepository;
            }
        }

        public GroupRepository GroupRepository
        {
            get
            {
                if (groupRepository == null)
                    return new GroupRepository(_dbContext);

                return groupRepository;
            }
        }

        public TagRepository TagRepository
        {
            get
            {
                if (tagRepository == null)
                    return new TagRepository(_dbContext);

                return tagRepository;
            }
        }

        public UserRepository UserRepository
        {
            get
            {
                if (userRepository == null)
                    return new UserRepository(_dbContext);

                return userRepository;
            }
        }

        public LanguageRepository LanguageRepository
        {
            get
            {
                if (languageRepository == null)
                    return new LanguageRepository(_dbContext);

                return languageRepository;
            }
        }

        public NotificationSettingRepository NotificationSettingRepository
        {
            get
            {
                if (notificationSettingRepository == null)
                    return new NotificationSettingRepository(_dbContext);

                return notificationSettingRepository;
            }
        }

        public HistoryGroupRepository HistoryGroupRepository
        {
            get
            {
                if (historyGroupRepository == null)
                    return new HistoryGroupRepository(_dbContext);

                return historyGroupRepository;
            }
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }
    }
}
