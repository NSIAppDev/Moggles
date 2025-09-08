using Moggles.Domain;
using Moggles.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Moggles.EndToEndTests.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Moggles.EndToEndTests.Helpers
{
    public static class FeatureFlagHandler
    {
        private static RestClient Client => RequestHelper.GetRestClient(Constants.BaseUrl, Constants.MogglesUser, Constants.MogglesPassword);
        public static string SmokeTestsApplicationId => GetApplicationProperties(Constants.SmokeTestsApplication)?.Id.ToString();

        public static IRestResponse GetApplications()
        {
            try
            {
                var request = RequestHelper.GetRequest("api/applications");
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                var response = Client.Execute(request);
                if (!response.IsSuccessful)
                {
                    Assert.Fail($"API call failed! Status: {response.StatusCode}, Message: {response.ErrorMessage}");
                }
                return response;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Test setup failed due to an exception: {ex.Message}");
                throw;
            }
        }
        public static Application GetApplicationProperties(string applicationName)
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

        public static IRestResponse GetFeatureToggles(string applicationId)
        {
            try
            {
                var request = RequestHelper.GetRequest("api/FeatureToggles");
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddParameter("applicationId", applicationId);
                return Client.Execute(request);

            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in GetFeatureToggles: {ex.Message}");
                throw;
            }
        }

        public static FeatureToggleViewModel GetFeatureToggleProperties(string applicationId, string featureToggleName)
        {
            try
            {
                var featureToggles = GetFeatureToggles(applicationId);
                var featureTogglesResultsOutput = JsonConvert.DeserializeObject<IEnumerable<FeatureToggleViewModel>>(featureToggles.Content);
                return featureTogglesResultsOutput.FirstOrDefault(x => x.ToggleName.Equals(featureToggleName));

            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in GetFeatureToggleProperties: {ex.Message}");
                throw;
            }
        }

        private static List<EnvironmentForFTDeserialized> GetFeatureToggleEnvironments(string applicationId)
        {
            try
            {
                var request = new RestRequest($"api/FeatureToggles/environments", Method.GET);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddParameter("applicationId", applicationId);
                var response = Client.Execute(request);
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
        public static IRestResponse DeleteFeatureToggles(string applicationId, string featureToggleId, string reasonToDelete)
        {
            try
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
            catch (Exception ex)
            {
                Assert.Fail($"Exception in DeleteFeatureToggles: {ex.Message}");
                throw;
            }
        }

        public static IRestResponse AddFeatureToggles(string applicationId, string featureToggleName)
        {
            try
            {
                var body = new AddFeatureToggleModel
                {
                    ApplicationId = new Guid(applicationId),
                    FeatureToggleName = featureToggleName
                };
                var request = RequestHelper.GetRequest("api/FeatureToggles/addFeatureToggle", body, Method.POST);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                return Client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in AddFeatureToggles: {ex.Message}");
                throw;
            }
        }

        public static IRestResponse DeleteApplication(string applicationId)
        {
            try
            {
                var request = RequestHelper.GetRequest("api/applications", Method.DELETE);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddParameter("id", applicationId);
                return Client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in DeleteApplication: {ex.Message}");
                throw;
            }
        }
        
        public static IRestResponse DeleteFeatureToggleEnvironment(string applicationId, string env)
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

                var request = RequestHelper.GetRequest("api/FeatureToggles/environments", body, Method.DELETE);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddJsonBody(body);
                var response = Client.Execute(request);

                if (!response.IsSuccessful)
                {
                    Assert.Fail($"[ERROR] Delete request failed: {response.StatusCode} - {response.Content}");
                    return response;
                }
                return response;
            }
            catch (Exception ex) {
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = $"Exception occurred: {ex.Message}"
                };
            }
        }

        public static void UpdateFeatureFlag(FeatureToggleUpdateModel featureToggleUpdateModel)
        {
            try
            {
                var request = RequestHelper.GetRequest("api/featuretoggles", featureToggleUpdateModel, Method.PUT);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                Client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in UpdateFeatureFlag: {ex.Message}");
                throw;
            }
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

        public static void ReactivateApp(UpdateApplicationModel updateApplicationModel)
        {
            try
            {
                var request = RequestHelper.GetRequest("api/applications/update", updateApplicationModel, Method.PUT);
                request.AddHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddJsonBody(updateApplicationModel);
                Client.Execute(request);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in ReactivateApp: {ex.Message}");
                throw;
            }
        }

    }
}
