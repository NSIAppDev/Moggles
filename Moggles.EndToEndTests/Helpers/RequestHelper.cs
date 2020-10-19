using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Threading;

namespace Moggles.EndToEndTests.Helpers
{
    public static class RequestHelper
    {
        private static readonly ReaderWriterLockSlim MethodLock = new ReaderWriterLockSlim();

        public static RestRequest GetRequest(string resource, Method method = Method.GET)
        {
            var request = new RestRequest(resource, method) { RequestFormat = DataFormat.Json };
            return request;
        }

        public static RestRequest GetRequest(string resource, object bodyParameter, Method method)
        {
            var request = new RestRequest(resource, method) { RequestFormat = DataFormat.Json };
            request.AddJsonBody(bodyParameter);
            return request;
        }

        public static RestClient GetRestClient(string url, string user = "", string password = "")
        {
            var client = new RestClient(url);

            if (user.Equals("") | password.Equals("")) return client;

            SetupAuthentication(url, user, password, client);

            return client;
        }

        public static void SetupAuthentication(string url, string user, string password, IRestClient client)
        {
            var credential = new CredentialCache
            {
                {new Uri(url), "NTLM", new NetworkCredential(user, password)}
            };

            client.Authenticator = new NtlmAuthenticator(credential);
            client.PreAuthenticate = true;
        }

        public static IRestResponse CreateRequest(this RestClient client, string resource, Method method = Method.GET)
        {
            MethodLock.EnterWriteLock();
            try
            {
                var request = GetRequest(resource, method);

                ServicePointManager.ServerCertificateValidationCallback =
                    (senderX, certificate, chain, sslPolicyErrors) => true;

                return client.Execute(request);
            }
            finally
            {
                MethodLock.ExitWriteLock();
            }
        }
    }
}
