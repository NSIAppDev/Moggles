using Microsoft.Playwright;
using Moggles.E2EPlaywrightTests.Helpers.Models;
using Moggles.Models;
using RestSharp;
using System.Text.Json;
using WGSHelpers.Authentication;

namespace Moggles.E2EPlaywrightTests.Helpers
{
    public class FeatureFlagHelper
    {
        private readonly IRestClient _client;
        public FeatureFlagHelper(IRestClient client)
        {
            _client = client;
        }

        #region Applications
        public async Task<string?> GetSmokeTestsApplicationIdAsync(string applicationName)
        {
            var app = await GetApplicationProperties(applicationName);
            return app?.Id.ToString();
        }
        private Task<ApiResult<List<ApplicationDto>>> GetApplications()
        {
            return SafeApiCall<List<ApplicationDto>>(async () => 
                await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url + "api/applications",
                    ApiHelpers.HttpMethodType.Get,
                    null,
                    true
                )
            );
        }
        public async Task<ApplicationDto?> GetApplicationProperties(string applicationName)
        {
            var appsResult = await GetApplications();

            if (!appsResult.Success)
            {
                Assert.Fail($"Exception in GetApplicationProperties: {appsResult.ErrorMessage}");
                return null;
            }
            var app = appsResult.Data.FirstOrDefault(x =>
                x.AppName.Equals(applicationName, StringComparison.OrdinalIgnoreCase));

            if (app == null)
            {
                Console.WriteLine($"Warning: Application '{applicationName}' not found in API response.");
                return null;
            }

            return app;
        }
        public async Task<ApiResult<ApplicationDto>> ReactivateApp(UpdateApplicationModel updateApplicationModel)
        {
            return await SafeApiCall<ApplicationDto>(async () =>
            {
                var resp = await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url + "api/applications/update",
                    ApiHelpers.HttpMethodType.Put,
                    updateApplicationModel,
                    true
                );

                return resp as IAPIResponse ?? throw new InvalidOperationException("SendApiRequestAsync did not return IAPIResponse");
            });
        }
        public async Task<ApiResult<bool>> DeleteApplication(string applicationId)
        {
            return await SafeApiCall<bool>(async () =>
            {
                var resp = await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url+ $"/api/applications?id={applicationId}",
                    ApiHelpers.HttpMethodType.Delete,
                    null,
                    true
                );
                return resp as IAPIResponse ?? throw new InvalidOperationException("SendApiRequestAsync did not return IAPIResponse");
            }); 
        }
        #endregion

        #region featureToggles 
        private async Task<ApiResult<List<FeatureToggleViewModel>>> GetFeatureToggles(string applicationId)
        {
            return await SafeApiCall<List<FeatureToggleViewModel>>(async () =>
            {
                return await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url + "api/FeatureToggles" + $"?applicationId={applicationId}",
                    ApiHelpers.HttpMethodType.Get,
                    null,
                    true
                );
            });
        }
        public async Task<FeatureToggleViewModel> GetFeatureToggleProperties(string applicationId, string featureToggleName)
        {
            var featureTogglesResult = await GetFeatureToggles(applicationId);

            if (!featureTogglesResult.Success)
            {
                Assert.Fail($"Failed to get feature toggle properties: {featureTogglesResult.ErrorMessage}");
                return null;
            }

            var featureToggle = featureTogglesResult.Data?
                .FirstOrDefault(x => x.ToggleName.Equals(featureToggleName, StringComparison.OrdinalIgnoreCase));

            if (featureToggle == null)
            {
                Assert.Fail($"Feature toggle '{featureToggleName}' not found for application {applicationId}");
            }

            return featureToggle;
        }
        private async Task<ApiResult<List<EnvironmentForFTDeserialized>>> GetFeatureToggleEnvironments(string applicationId)
        {
            return await SafeApiCall<List<EnvironmentForFTDeserialized>>(async () =>
            {
                var response = await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url + "api/FeatureToggles/environments" + $"?applicationId={applicationId}",
                    ApiHelpers.HttpMethodType.Get,
                    null,
                    true
                );
                var rawBody = await response.TextAsync();
                Console.WriteLine($"Raw API response: '{rawBody}'");

                return response;
            });
        }
        public async Task<ApiResult<bool>> DeleteFeatureToggleEnvironment(string applicationId, string env)
        {
            var envsForApp = await GetFeatureToggleEnvironments(applicationId);
            if (envsForApp == null || !envsForApp.Data.Any(x => x.EnvName.ToLower() == env.ToLower()))
                return null;
            var body = new DeleteEnvironmentModel
            {
                ApplicationId = new Guid(applicationId),
                EnvName = env
            };
            return await SafeApiCall<bool>(async () =>
            {
                var resp = await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url + "api/FeatureToggles/environments",
                    ApiHelpers.HttpMethodType.Delete,
                    body,
                    true
                );
                return resp as IAPIResponse ?? throw new InvalidOperationException("SendApiRequestAsync did not return IAPIResponse");
            });
        }
        
        public async Task<ApiResult<bool>> DeleteFeatureToggles(string applicationId, string featureToggleId, string reasonToDelete)
        {
            var body = new DeleteFeatureToggleModel
            {
                ApplicationId = new Guid(applicationId),
                FeatureToggleId = new Guid(featureToggleId),
                Reason = reasonToDelete
            };
            
            return await SafeApiCall<bool>(async () =>
            {
                var resp = await ApiHelpers.SendApiRequestAsync(
                    TestSuiteSetup.Url + "api/FeatureToggles",
                    ApiHelpers.HttpMethodType.Delete,
                    body,
                    true
                );
                return resp as IAPIResponse ?? throw new InvalidOperationException("SendApiRequestAsync did not return IAPIResponse");
            });
        }
        #endregion

        #region General methods
        private async Task<ApiResult<T>> SafeApiCall<T>(Func<Task<IAPIResponse>> apiCall)
        {
            try
            {
                var response = await apiCall();

                var result = new ApiResult<T>
                {
                    Status = response.Status,
                    StatusText = response.StatusText
                };

                if (response.Status >= 200 && response.Status < 300)
                {
                    try
                    {
                        // Only attempt to parse if content exists
                        var rawBody = await response.TextAsync();

                        if (!string.IsNullOrWhiteSpace(rawBody) 
                            && rawBody.TrimStart().StartsWith("{") 
                            || rawBody.TrimStart().StartsWith("["))
                        {
                            result.Data = await response.JsonAsync<T>(
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        else
                        {
                            result.Data = default; // no content
                        }
                        result.Success = true;
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"JSON parsing failed: {ex.Message}";
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = $"API returned error: {response.Status} {response.StatusText}";
                }
                
                return result;
            }
            catch (Exception ex)
            {
                return new ApiResult<T>
                {
                    Success = false,
                    Status = 500,
                    StatusText = "InternalError",
                    ErrorMessage = $"Exception occurred: {ex.Message}",
                    Data = default
                };
            }
        }

        #endregion
    }
}
