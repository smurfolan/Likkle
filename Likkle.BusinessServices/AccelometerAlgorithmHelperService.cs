using System;
using System.Device.Location;
using System.Linq;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class AccelometerAlgorithmHelperService : IAccelometerAlgorithmHelperService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IConfigurationWrapper _configuration;

        public AccelometerAlgorithmHelperService(
            ILikkleUoW unitOfWork, 
            IConfigurationWrapper configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public double SecondsToClosestBoundary(double latitude, double longitude)
        {
            var walkingSpeed = this._configuration.PersonWalkingSpeedInKmh;

            var currentLocation = new GeoCoordinate(latitude, longitude);

            var resultingDistance = Int32.MaxValue*1.0;

            var allAvailableAreas = this._unitOfWork.AreaRepository.GetAreas().Where(a => a.IsActive).ToList();

            if (!allAvailableAreas.Any())
                return 3600; // Default if no areas area available yet in the system

            foreach (var area in allAvailableAreas)
            {
                // Distance from current location to area center
                var d1 = currentLocation.GetDistanceTo(new GeoCoordinate(area.Latitude, area.Longitude));

                // Radius of the area
                var d2 = (int) area.Radius;

                var distance = Math.Abs(d2 - d1); // In this way we consider boundaries not only in which we fall but also the ones we are outside of

                if (distance < resultingDistance)
                    resultingDistance = distance;
            }

            var actualSpeed = (walkingSpeed * 1000.0) / 3600; // Speed in m/s

            return resultingDistance/actualSpeed;
        }
    }
}
