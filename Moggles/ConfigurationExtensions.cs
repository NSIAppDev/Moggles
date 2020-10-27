using Microsoft.Extensions.Configuration;

namespace Moggles
{
    public static class ConfigurationExtensions
    {
        public static bool UseJwt(this IConfiguration config) =>
            !string.IsNullOrEmpty(config["Jwt:TokenSigningKey"]);

        public static bool UseAkv(this IConfiguration config) =>
            !(string.IsNullOrEmpty(config["Security:AzureADCertThumbprint"])
              || string.IsNullOrEmpty(config["Security:AzureADApplicationId"]));
    }
}
