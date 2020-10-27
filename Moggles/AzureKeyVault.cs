using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Moggles
{
    public class AzureKeyVault
    {
        private static ClientAssertionCertificate AssertionCert { get; set; }
        
        public static void Configure(IConfigurationBuilder config)
        {
            var builtConfig = config.Build();

            if (!builtConfig.UseAkv())
                return;

            var client = GetKeyVaultClient(builtConfig["Security:AzureADCertThumbprint"],
                builtConfig["Security:AzureADApplicationId"]);

            config.AddAzureKeyVault(
                $"https://{builtConfig["Security:KeyVaultName"]}.vault.azure.net/", client,
                new DefaultKeyVaultSecretManager());
        }

        private static KeyVaultClient GetKeyVaultClient(string thumbprint, string clientApplicationId) =>
            GetCertification(thumbprint, clientApplicationId)
                ? new KeyVaultClient(GetAccessToken)
                : FallbackToToken();

        private static bool GetCertification(string thumbprint, string clientApplicationId)
        {
            var clientAssertionCertPfx = FindCertificateByThumbprint(thumbprint);
            if (clientAssertionCertPfx == null)
                return false;

            AssertionCert = new ClientAssertionCertificate(clientApplicationId, clientAssertionCertPfx);
            return true;
        }

        private static KeyVaultClient FallbackToToken()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            return keyVaultClient;
        }

        private static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                return certificateCollection.Count == 0
                    ? null
                    : certificateCollection[0];
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        private static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, AssertionCert);

            return result.AccessToken;
        }
    }
}
