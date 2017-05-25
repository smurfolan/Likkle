using System;

namespace Likkle.BusinessServices
{
    public class AccelometerAlgorithmHelperService : IAccelometerAlgorithmHelperService
    {
        public double SecondsToClosestBoundary(double latitude, double longitude)
        {
            // TODO: Implement calculations. Consider being not only insida area but alos outside of it.
            return 32.4;
        }
    }
}
