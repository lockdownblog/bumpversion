namespace BumpVersion.Configuration
{
    using System.Collections.Generic;
    using System.IO;
    using Tomlyn;
    using Tomlyn.Model;

    public class BumpConfigurationManager : IBumpConfigurationManager
    {
        private string configurationFilePath;
        private string configurationFileContent;
        private TomlTable bumpversionConfiguration;

        public void ReadConfig(string configurationFilePath)
        {
            if (this.bumpversionConfiguration is null)
            {
                this.configurationFilePath = configurationFilePath;
                this.configurationFileContent = File.ReadAllText(configurationFilePath);
                this.bumpversionConfiguration = Toml.Parse(this.configurationFileContent).ToModel()["bumpversion"] as TomlTable;
            }
        }

        public GlobalConfiguration GetGlobalConfiguration()
        {
            this.bumpversionConfiguration.TryGetValue("parse", out var parseObject);
            this.bumpversionConfiguration.TryGetValue("new_version", out var newVersionObject);
            this.bumpversionConfiguration.TryGetValue("serialize", out var serializeObject);
            this.bumpversionConfiguration.TryGetValue("message", out var messageObj);
            this.bumpversionConfiguration.TryGetValue("commit_args", out var commitArgsObj);
            this.bumpversionConfiguration.TryGetValue("tag_name", out var tagNameObj);
            var globalConfiguration = new GlobalConfiguration
            {
                CurrentVersion = this.bumpversionConfiguration["current_version"] as string,
                Parse = parseObject as string ?? GlobalConfiguration.Defaults.Parse,
                Serialize = serializeObject as string ?? GlobalConfiguration.Defaults.Serialize,
                Message = messageObj as string ?? GlobalConfiguration.Defaults.Message,
                CommitArgs = commitArgsObj as string,
                TagName = tagNameObj as string ?? GlobalConfiguration.Defaults.TagName,
                NewVersion = newVersionObject as string,
            };

            if (this.bumpversionConfiguration.TryGetValue("tag", out var tagObject))
            {
                globalConfiguration.Tag = (bool)tagObject;
            }
            else
            {
                globalConfiguration.Tag = GlobalConfiguration.Defaults.Tag;
            }

            if (this.bumpversionConfiguration.TryGetValue("sign_tags", out var signTagsObject))
            {
                globalConfiguration.SignTags = (bool)signTagsObject;
            }
            else
            {
                globalConfiguration.SignTags = GlobalConfiguration.Defaults.SignTags;
            }

            if (this.bumpversionConfiguration.TryGetValue("commit", out var commitObject))
            {
                globalConfiguration.Commit = (bool)commitObject;
            }
            else
            {
                globalConfiguration.Commit = GlobalConfiguration.Defaults.Commit;
            }

            return globalConfiguration;
        }

        public List<FileConfiguration> GetFileConfigurations()
        {
            var fileConfigurations = new List<FileConfiguration>();
            if (this.bumpversionConfiguration.TryGetToml("file", out var tomlObject))
            {
                var fileTables = tomlObject as TomlTable;

                if (fileTables.TryGetValue("file", out var fileName))
                {
                    fileConfigurations.Add(this.GetFileConfiguration(fileTables));
                }
                else
                {
                    for (int idx = 0; idx < int.MaxValue; idx++)
                    {
                        if (fileTables.TryGetToml(idx.ToString(), out var tomlFileObject))
                        {
                            var tomlFile = tomlFileObject as TomlTable;
                            fileConfigurations.Add(this.GetFileConfiguration(tomlFile));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return fileConfigurations;
        }

        public void UpdateConfigurationVersion(string currentVersion, string newVersion)
        {
            this.configurationFileContent = this.configurationFileContent.Replace($"\"{currentVersion}\"", $"\"{newVersion}\"");
            File.WriteAllText(this.configurationFilePath, this.configurationFileContent);
        }

        private FileConfiguration GetFileConfiguration(TomlTable bumpversionFileConfiguration)
        {
            bumpversionFileConfiguration.TryGetValue("parse", out var parseObj);
            bumpversionFileConfiguration.TryGetValue("serialize", out var serializeObj);
            bumpversionFileConfiguration.TryGetValue("search", out var searchObj);
            bumpversionFileConfiguration.TryGetValue("replace", out var replaceObj);

            var fileConfiguration = new FileConfiguration
            {
                File = bumpversionFileConfiguration["file"] as string,
                Parse = parseObj as string ?? FileConfiguration.Defaults.Parse,
                Serialize = serializeObj as string ?? FileConfiguration.Defaults.Serialize,
                Search = searchObj as string ?? FileConfiguration.Defaults.Search,
                Replace = replaceObj as string ?? FileConfiguration.Defaults.Replace,
            };

            return fileConfiguration;
        }
    }
}
