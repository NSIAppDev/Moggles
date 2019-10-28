using Newtonsoft.Json;
using RestSharp;
using System;

namespace Moggles.EndToEndTests.TestFramework
{
    public class ApiHelpers
    {
        private RestClient _client;

        public void DeleteFeatureToggle(Guid id, Guid applicationId)
        {
            _client = RestHelper.RestClient();
            var data = new RemoveFeatureT
            {
                CustomerNumber = CustomerNumber,
                MaterialNumber = MaterialNumber,
                ProgramType = ProgramType
            };
            var inputData = JsonConvert.SerializeObject(data);
            var request = RestHelper.RestRequest(ApiResources.DeleteMaterialFromProgram, Method.POST);
            request.AddJsonBody(inputData);

            var response = _client.Execute(request);
        }
    }
}
