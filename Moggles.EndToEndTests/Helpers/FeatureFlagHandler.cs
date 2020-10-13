using Moggles.Models;
using MogglesEndToEndTests.TestFramework;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace Moggles.EndToEndTests.Helpers
{
    public static class FeatureFlagHandler
    {
        private static RestClient Client => RequestHelper.GetRestClient(Constants.FeatureToggleUrl, Constants.MogglesUser, Constants.MogglesPassword);

        private static IRestResponse GetFeatureToggles(string applicationId)
        {
            var request = RequestHelper.GetRequest("api/FeatureToggles");
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            request.AddParameter("applicationId", applicationId);

            return Client.Execute(request);
        }

        public static FeatureToggleViewModel GetFeatureToggleProperties(string applicationId, string featureToggleId)
        {
            var featureToggles = GetFeatureToggles(applicationId);
            var featureTogglesResultsOutput = JsonConvert.DeserializeObject<IEnumerable<FeatureToggleViewModel>>(featureToggles.Content);
            return featureTogglesResultsOutput.FirstOrDefault(x => x.Id.Equals(featureToggleId));
        }

    }
}
