namespace Moggles.E2EPlaywrightTests.Helpers
{
    public static class AuthProfile
    {
        public static readonly Dictionary<string, (string Username, string Password, string EncryptedFile)> Profiles =
            new()
            {
                ["admin"] = ("user", "pass", "admin.json.enc")
            };

        public static void SetCredentials(string role, string username, string password)
        {
            if (!Profiles.ContainsKey(role))
                throw new ArgumentException($"Role {role} not found in profiles.");

            var existing = Profiles[role];
            Profiles[role] = (username, password, existing.EncryptedFile);
        }

        public static string GetEncryptedFileFor(string role) =>
            Profiles.TryGetValue(role, out var profile)
                ? profile.EncryptedFile
                : throw new ArgumentException($"Unknown role: {role}");

        public static IEnumerable<string> AllRoles => Profiles.Keys;
    }
}
