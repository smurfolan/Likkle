using System;
using System.Collections.Generic;
using AutoMapper;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class SubscriptionSettingsService : ISubscriptionSettingsService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionSettingsService(ILikkleUoW unitOfWork, IConfigurationProvider configurationProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = configurationProvider.CreateMapper();
        }

        public IEnumerable<Guid> GroupsForUserAroundCoordinatesBasedOnUserSettings(
            Guid userId, 
            double latitude, 
            double longitude)
        {
            return new List<Guid>();
        }
    }
}
