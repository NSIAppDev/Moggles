using Moggles.E2EPlaywrightTests.Helpers;
using NSTestFrameworkDotNetCoreApi;
using WGSHelpers.Authentication;
using WGSHelpers.Helpers;

namespace Moggles.E2EPlaywrightTests
{
    [TestClass]
    public class TestSuiteSetup
    {
        public static string Url;
        public static string KeyVaultName;
        public static string EncryptionKey;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Task.Run(async () => await AssemblyInitAsync(context)).GetAwaiter().GetResult();
        }

        public static async Task AssemblyInitAsync(TestContext context)
        {
            if (context.Properties.Contains("webAppUrl"))
            {
                Console.WriteLine("webAppUrl found: " + context.Properties["webAppUrl"]);
            }
            else
            {
                Console.WriteLine("webAppUrl NOT found!");
            }

            Url = context.Properties["webAppUrl"]?.ToString() ?? throw new InvalidOperationException();

            KeyVaultName = "AppDev-Dev";
            KeyVaultHelper.InitializeVault(KeyVaultName);

            EncryptionKey = KeyVaultHelper.GetSecret("AuthTestingEncryptionKey");

            // Set credentials from Key Vault into AuthProfiles
            AuthProfile.SetCredentials("admin",
                KeyVaultHelper.GetSecret("sa3925SmokeTestMogglesAdmin"),
                KeyVaultHelper.GetSecret("sa3925SmokeTestMogglesAdminPass"));

            await SaveAuthStateIfFileDoesNotExist();
        }

        [AssemblyCleanup]
        public static void CleanupAuthFiles()
        {
            AuthHelper.CleanupAuthStateFiles();
            BrowserManager.DisposeAsync();
        }

        private static async Task SaveAuthStateIfFileDoesNotExist()
        {
            foreach (var role in AuthProfile.AllRoles)
            {
                var profile = AuthProfile.Profiles[role];
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auth", profile.EncryptedFile)))
                {
                    await AuthHelper.SaveAndEncryptAuthStateWithSpecifiedUserRoleAsync(
                        role,
                        profile.Username,
                        profile.Password,
                        profile.EncryptedFile,
                        Url,
                        EncryptionKey
                    );
                }
            }
        }
    }
}
