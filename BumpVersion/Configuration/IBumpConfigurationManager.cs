namespace BumpVersion.Configuration
{
    using System.Collections.Generic;

    public interface IBumpConfigurationManager
    {
        void ReadConfig(string configurationFilePath);

        List<FileConfiguration> GetFileConfigurations();

        GlobalConfiguration GetGlobalConfiguration();

        void UpdateConfigurationVersion(string currentVersion, string newVersion);
    }
}