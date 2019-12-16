using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using MogglesEndToEndTests.TestFramework;
using RestSharp.Authenticators;

namespace Moggles.EndToEndTests.SmokeTests
{
    public static class Test
    {
        private static RestClient Client => new RestClient(Constants.BaseUrl);
        public  static void AddFeatureFlagg(FeatureToggleAddModel featureToggleAddModel)
        {
            var request = GetRequest("api/featuretoggles/addFeatureToggle", featureToggleAddModel, Method.POST);
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            Client.Authenticator = new NtlmAuthenticator("rtoadere", "RAMTOASafety2");
            Client.Execute(request);
        }

        public static RestRequest GetRequest(string resource, object bodyParameter, Method method = Method.GET)
        {
            var request = new RestRequest(resource, method) { RequestFormat = DataFormat.Json };
            request.AddJsonBody(bodyParameter);
            return request;
        }
    }

    public class FeatureToggleAddModel
    {
        public string ApplicationId { get; set; }
        public string FeatureToggleName { get; set; }
        public string Notes { get; set; }
        public bool IsPermanent { get; set; }
    }
}
