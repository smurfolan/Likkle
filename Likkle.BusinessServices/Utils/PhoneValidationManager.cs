using System;
using System.Threading.Tasks;
using RestSharp;

namespace Likkle.BusinessServices.Utils
{
    public class PhoneValidationManager : IPhoneValidationManager
    {
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly string _phoneNumberValidationApiKey;
        private readonly RestClient _restClient;

        public PhoneValidationManager(IConfigurationWrapper configurationWrapper)
        {
            this._configurationWrapper = configurationWrapper;
            _phoneNumberValidationApiKey = this._configurationWrapper.NumverifyApiKey;

            _restClient = new RestClient(this._configurationWrapper.NumverifyApiRoot);
        }

        public bool PhoneNumberIsValid(string phoneNumber)
        {
            var request = new RestRequest($"/api/validate?access_key={this._phoneNumberValidationApiKey}&number={phoneNumber}&country_code=&format=1", Method.GET);

            // execute the request
            var tcs = new TaskCompletionSource<PhoneNumberValidationResponse>();

            _restClient.ExecuteAsync<PhoneNumberValidationResponse>(request, restResponse =>
            {
                tcs.SetResult(restResponse.Data);
            });

            var validationResult = tcs.Task.Result.valid;

            return Convert.ToBoolean(validationResult);
        }
    }
}
