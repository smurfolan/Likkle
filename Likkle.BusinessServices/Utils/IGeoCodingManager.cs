using Likkle.BusinessEntities.Requests;

namespace Likkle.BusinessServices.Utils
{
    public interface IGeoCodingManager
    {
        string GetApproximateAddress(NewAreaRequest newArea);
    }
}
