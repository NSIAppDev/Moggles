using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using MogglesEndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.TestFramework
{
    public class RestHelper
    {
        public static RestClient RestClient()
        {
            var client = new RestClient(Constants.BaseUrl);

            var credential = new CredentialCache
            {
                {new Uri(Constants.BaseUrl), "NTLM", new NetworkCredential("SAFETY\\rtoadere", "RAMTOASafety2")}
            };

            client.Authenticator = new NtlmAuthenticator(credential);
            client.PreAuthenticate = true;
            return client;
        }

        public static RestRequest RestRequest(string endpoint, Method method)
        {
            var restRequest = new RestRequest(endpoint, method);
            return restRequest;
        }
    }
}
