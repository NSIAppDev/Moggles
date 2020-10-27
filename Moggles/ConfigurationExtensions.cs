using Microsoft.Extensions.Configuration;

namespace Moggles
{
    public static class ConfigurationExtensions
    {
        public static bool UseJwt(this IConfiguration config) => UseAkv(config) ||
            !string.IsNullOrEmpty(config["MogglesTokenSigningKey"]);

        public static bool UseAkv(this IConfiguration config) =>
            !(string.IsNullOrEmpty(config["Security:AzureADCertThumbprint"])
              || string.IsNullOrEmpty(config["Security:AzureADApplicationId"]));

        public static string GetTokenSigningKey(this IConfiguration config)
        {
            var tokenSigningKey = config["MogglesTokenSigningKey"];

            if (string.IsNullOrEmpty(tokenSigningKey))
                tokenSigningKey = config["Jwt:TokenSigningKey"];

            return tokenSigningKey;
        }
    }
}
