using Moggles.Domain;
using Moggles.Models;
using Newtonsoft.Json;
using NSTestFrameworkDotNetCoreApi.RestSharp;
using RestSharp;
using System.Net;

namespace Moggles.E2EPlaywrightTests.Helpers
{
    public class FeatureFlagHelper
    {
        private readonly IRestClient _client;
        public FeatureFlagHelper(IRestClient client)
        {
            _client = client;
        }

        public string SmokeTestsApplicationId => GetApplicationProperties(Constants.SmokeTestsApplication)?.Id.ToString();

        public IRestResponse GetApplications()
        {
            var request = BaseHelper.GetRequest("api/applications");
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            return _client.Execute(request);
        }
        public Application GetApplicationProperties(string applicationName)
        {
            try
            {
                var applications = GetApplications();
                if (!applications.IsSuccessful)
                {
                    Assert.Fail($"API call failed! Status: {applications.StatusCode}, Message: {applications.ErrorMessage}");
                }
                var applicationsResultsOutput = JsonConvert.DeserializeObject<IEnumerable<Application>>(applications.Content);
                var response = applicationsResultsOutput.FirstOrDefault(x => x.AppName.Equals(applicationName));
                if (response == null)
                {
                    throw new Exception($"Application '{applicationName}' not found.");
                }
                return response;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in GetApplicationProperties: {ex.Message}");
                throw;
            }
        }
        private List<EnvironmentForFTDeserialized> GetFeatureToggleEnvironments(string applicationId)
        {
            try
            {
                var request = new RestRequest($"api/FeatureToggles/environments", Method.GET);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddParameter("applicationId", applicationId);
                var response = _client.Execute(request);
                if (!response.IsSuccessful)
                {
                    return new List<EnvironmentForFTDeserialized>(); // return empty list
                }
                var data = JsonConvert.DeserializeObject<List<EnvironmentForFTDeserialized>>(response.Content);
                if (data == null || !data.Any()) return new List<EnvironmentForFTDeserialized>();
                return data;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in GetFeatureToggleEnvironments: {ex.Message}");
                throw;
            }
        }
        public IRestResponse DeleteFeatureToggles(string applicationId, string featureToggleId, string reasonToDelete)
        {
            try
            {
                var body = new DeleteFeatureToggleModel
                {
                    ApplicationId = new Guid(applicationId),
                    FeatureToggleId = new Guid(featureToggleId),
                    Reason = reasonToDelete
                };
                var request = BaseHelper.GetRequest("api/FeatureToggles", body, Method.DELETE);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                return _client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in DeleteFeatureToggles: {ex.Message}");
                throw;
            }
        }

        public IRestResponse AddFeatureToggles(string applicationId, string featureToggleName)
        {
            try
            {
                var body = new AddFeatureToggleModel
                {
                    ApplicationId = new Guid(applicationId),
                    FeatureToggleName = featureToggleName
                };
                var request = BaseHelper.GetRequest("api/FeatureToggles/addFeatureToggle", body, Method.POST);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                return _client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in AddFeatureToggles: {ex.Message}");
                throw;
            }
        }

        public IRestResponse DeleteApplication(string applicationId)
        {
            try
            {
                var request = BaseHelper.GetRequest("api/applications", Method.DELETE);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddParameter("id", applicationId);
                return _client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in DeleteApplication: {ex.Message}");
                throw;
            }
        }

        public IRestResponse DeleteFeatureToggleEnvironment(string applicationId, string env)
        {
            try
            {
                var envsForApp = GetFeatureToggleEnvironments(applicationId);
                if (envsForApp == null || envsForApp.Any(x => x.EnvName.ToLower() == env.ToLower()))
                    throw new Exception("Data not found, cannot proceed with delete.");

                var body = new DeleteEnvironmentModel
                {
                    ApplicationId = new Guid(applicationId),
                    EnvName = env
                };

                var request = BaseHelper.GetRequest("api/FeatureToggles/environments", body, Method.DELETE);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddJsonBody(body);
                var response = _client.Execute(request);

                if (!response.IsSuccessful)
                {
                    Assert.Fail($"[ERROR] Delete request failed: {response.StatusCode} - {response.Content}");
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = $"Exception occurred: {ex.Message}"
                };
            }
        }

        public void UpdateFeatureFlag(FeatureToggleUpdateModel featureToggleUpdateModel)
        {
            try
            {
                var request = BaseHelper.GetRequest("api/featuretoggles", featureToggleUpdateModel, Method.PUT);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                _client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in UpdateFeatureFlag: {ex.Message}");
                throw;
            }
        }

        public FeatureToggleUpdateModel SetFeatureToggleUpdateModel(FeatureToggleViewModel featureToggleProperties, string applicationId, bool enabled, string environment)
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

        public void ReactivateApp(UpdateApplicationModel updateApplicationModel)
        {
            try
            {
                var request = BaseHelper.GetRequest("api/applications/update", updateApplicationModel, Method.PUT);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddJsonBody(updateApplicationModel);
                _client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in ReactivateApp: {ex.Message}");
                throw;
            }
        }

    }
}
