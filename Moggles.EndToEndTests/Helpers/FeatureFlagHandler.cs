using Moggles.Domain;
using Moggles.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Moggles.EndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.Helpers
{
    public static class FeatureFlagHandler
    {
        private static RestClient Client => RequestHelper.GetRestClient(Constants.BaseUrl, Constants.MogglesUser, Constants.MogglesPassword);
        public static string SmokeTestsApplicationId => FeatureFlagHandler.GetApplicationProperties(Constants.SmokeTestsApplication)?.Id.ToString();

        public static IRestResponse GetApplications()
        {
            var request = RequestHelper.GetRequest("api/applications");
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            return Client.Execute(request);
        }
        public static Application GetApplicationProperties(string applicationName)
        {
            var applications = GetApplications();
            var applicationsResultsOutput = JsonConvert.DeserializeObject<IEnumerable<Application>>(applications.Content);
            return applicationsResultsOutput.FirstOrDefault(x => x.AppName.Equals(applicationName));
        }

        public static IRestResponse GetFeatureToggles(string applicationId)
        {
            var request = RequestHelper.GetRequest("api/FeatureToggles");
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            request.AddParameter("applicationId", applicationId);
            return Client.Execute(request);
        }

        public static FeatureToggleViewModel GetFeatureToggleProperties(string applicationId, string featureToggleName)
        {
            var featureToggles = GetFeatureToggles(applicationId);
            var featureTogglesResultsOutput = JsonConvert.DeserializeObject<IEnumerable<FeatureToggleViewModel>>(featureToggles.Content);
            return featureTogglesResultsOutput.FirstOrDefault(x => x.ToggleName.Equals(featureToggleName));
        }
        public static IRestResponse DeleteFeatureToggles(string applicationId, string featureToggleId, string reasonToDelete)
        {
            var body = new DeleteFeatureToggleModel
            {
                ApplicationId = new Guid(applicationId),
                FeatureToggleId = new Guid(featureToggleId),
                Reason = reasonToDelete
            };
            var request = RequestHelper.GetRequest("api/FeatureToggles", body, Method.DELETE);
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            return Client.Execute(request);
        }
        public static IRestResponse DeleteApplication(string applicationId)
        {
            var request = RequestHelper.GetRequest("api/applications", Method.DELETE);
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            request.AddParameter("id", applicationId);
            return Client.Execute(request);
        }
        public static void UpdateFeatureFlag(FeatureToggleUpdateModel featureToggleUpdateModel)
        {
            var request = RequestHelper.GetRequest("api/featuretoggles", featureToggleUpdateModel, Method.PUT);
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            Client.Execute(request);
        }
        public static FeatureToggleUpdateModel SetFeatureToggleUpdateModel(FeatureToggleViewModel featureToggleProperties, string applicationId, bool enabled, string environment)
        {
            return new FeatureToggleUpdateModel
            {
                ApplicationId = new Guid(applicationId),
                Id = featureToggleProperties.Id,
                FeatureToggleName = featureToggleProperties.ToggleName,
                Notes = featureToggleProperties.Notes,
                UserAccepted = featureToggleProperties.UserAccepted,
                IsPermanent = enabled,
                Statuses = new List<FeatureToggleStatusUpdateModel>
                { new FeatureToggleStatusUpdateModel{
                    Enabled = enabled,
                    Environment = environment}
                },
                WorkItemIdentifier = featureToggleProperties.WorkItemIdentifier
                
            };
        }

    }
}
