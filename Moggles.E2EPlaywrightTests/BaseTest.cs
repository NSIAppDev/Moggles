using Moggles.E2EPlaywrightTests.Helpers;
using Moggles.E2EPlaywrightTests.Pages;
using NSTestFrameworkDotNetCoreApi.RestSharp;
using RestSharp;
using WGSHelpers;
using WGSHelpers.Authentication;
using WGSHelpers.Helpers;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

public abstract class BaseTest : BaseTestPW
{
    protected string Role { get; private set; } = null!;
    protected string EncryptedFileForUser { get; private set; } = null!;
    protected RestClient Client { get; private set; } = null!;
    protected FeatureFlagHelper FeatureFlagHelper { get; private set; } = null!;
    protected FeatureTogglesPage FeatureTogglesPage { get; private set; }

    [TestInitialize]
    public virtual async Task SetupAsync()
    {
        GetUserRole();
        EncryptedFileForUser = AuthProfile.GetEncryptedFileFor(Role);

        _context = await BrowserManager.SetupWithAuthAsync(
            encryptedAuthFile: EncryptedFileForUser,
            userRole: Role,
            isHeadless: false
        );
        _page = await _context.NewPageAsync();
        ApiHelpers.APIContext = _page.APIRequest;

        ClientInitializer();
        FeatureFlagHelper = new FeatureFlagHelper(Client);

        PageInitializer();
    }

    private void ClientInitializer()
    {
        var (username, password, _) = AuthProfile.Profiles[Role];
        Client = RequestHelper.GetRestClient(
            Constants.BaseUrl,
            username,
            password
        );
    }
    private void GetUserRole()
    {
        Role = TestContext.Properties["role"]?.ToString();
        if (string.IsNullOrEmpty(Role))
            throw new InvalidOperationException("TestUserContext.CurrentRole not set before SetupAsync.");
    }

    private void PageInitializer() {
        FeatureTogglesPage = new FeatureTogglesPage(_page);

    }
}