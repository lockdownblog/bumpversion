namespace BumpVersion.Configuration
{
    public class GlobalConfiguration
    {
        public string Parse { get; set; }

        public string Serialize { get; set; }

        public string CurrentVersion { get; set; }

        public string NewVersion { get; set; }

        public bool Tag { get; set; }

        public bool SignTags { get; set; }

        public string TagName { get; set; }

        public bool Commit { get; set; }

        public string Message { get; set; }

        public string CommitArgs { get; set; }

        public static class Defaults
        {
            public const string Parse = @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)";
            public const string Serialize = "{major}.{minor}.{patch}";
            public const bool Tag = false;
            public const bool SignTags = false;
            public const string TagName = "v{new_version}";
            public const bool Commit = false;
            public const string Message = "Bump version: {current_version} → {new_version}";
        }
    }
}
