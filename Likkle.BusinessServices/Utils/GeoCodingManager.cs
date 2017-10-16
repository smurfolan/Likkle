using System.Linq;
using System.Threading.Tasks;
using Likkle.BusinessEntities.Requests;
using RestSharp;

namespace Likkle.BusinessServices.Utils
{
    public class GeoCodingManager : IGeoCodingManager
    {
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly string _reverseGeocodingKey;
        private readonly RestClient _restClient;

        public GeoCodingManager(IConfigurationWrapper configurationWrapper)
        {
            _configurationWrapper = configurationWrapper;
            _reverseGeocodingKey = this._configurationWrapper.GoogleApiKeyForReverseGeoCoding;
            this._restClient = new RestClient(this._configurationWrapper.GoogleMapsApiRoot);
        }

        public string GetApproximateAddress(NewAreaRequest newArea)
        {
            var request = new RestRequest($"/api/geocode/json?language=en&latlng={newArea.Latitude},{newArea.Longitude}&key={this._reverseGeocodingKey}", Method.GET);

            // execute the request
            var tcs = new TaskCompletionSource<ReverseGeoCoordinateResponseRootObject>();

            _restClient.ExecuteAsync<ReverseGeoCoordinateResponseRootObject>(request, restResponse =>
            {
                tcs.SetResult(restResponse.Data);
            });

            var reverseGeoCoordinateResponse = tcs.Task.Result.results.FirstOrDefault();


            return (reverseGeoCoordinateResponse != null 
                ? $"{reverseGeoCoordinateResponse.formatted_address}"  
                : $"{newArea.Latitude}, {newArea.Longitude}") + $" ({(int)newArea.Radius} meters radius)";
        }
    }
}
