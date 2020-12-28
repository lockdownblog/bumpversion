namespace BumpVersion.Configuration
{
    public class FileConfiguration
    {
        public string File { get; set; }

        public string Parse { get; set; }

        public string Serialize { get; set; }

        public string Search { get; set; }

        public string Replace { get; set; }

        public static class Defaults
        {
            public const string Parse = @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)";
            public const string Serialize = "{major}.{minor}.{patch}";
            public const string Search = "{current_version}";
            public const string Replace = "{new_version}";
        }
    }
}
