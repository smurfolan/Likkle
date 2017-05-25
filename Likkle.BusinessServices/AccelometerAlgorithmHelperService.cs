using System;
using System.Device.Location;
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

            var resultingDistance = Int16.MaxValue*1.0;

            foreach (var area in this._unitOfWork.AreaRepository.GetAreas())
            {
                // Distance from current location to area center
                var d1 = currentLocation.GetDistanceTo(new GeoCoordinate(area.Latitude, area.Longitude));

                // Radius of the area
                var d2 = (int) area.Radius;

                var distance = Math.Abs(d2 - d1);

                if (distance < resultingDistance)
                    resultingDistance = distance;
            }



            // TODO: Implement calculations. Consider being not only insida area but alos outside of it.
            return resultingDistance;
        }
    }
}
