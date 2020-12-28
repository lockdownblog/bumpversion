namespace BumpVersion
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using global::BumpVersion.Configuration;
    using global::BumpVersion.Utils;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Tomlyn;
    using Tomlyn.Model;

    public class BumpVersion
    {
        private const string ConfigurationFileName = ".bumpversion.cfg";

        private readonly ILogger logger;
        private readonly IRepo repo;

        public BumpVersion(ILogger<BumpVersion> logger, IRepo repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        [Argument(0)]
        [Required]
        public string Part { get; }

        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
              .AddSingleton<IConsole>(PhysicalConsole.Singleton)
              .AddScoped<IRepo, Repo>()
              .AddLogging(configure => configure.AddConsole())
              .BuildServiceProvider();

            var app = new CommandLineApplication<BumpVersion>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);
            return app.Execute(args);
        }

        private int OnExecute()
        {
            GlobalConfiguration globalConfiguration = null;
            List<FileConfiguration> fileConfigurations = new List<FileConfiguration>();

            if (!this.repo.SetUpRepo())
            {
                this.logger.LogError("Sorry, the current folder is not a git repo!");
                return 1;
            }

            if (!this.repo.IsClean)
            {
                this.logger.LogError("Sorry, your repo is not clean!");
                return 1;
            }

            var bumpVersionConfigFileContent = File.ReadAllText(ConfigurationFileName);

            var doc = Toml.Parse(bumpVersionConfigFileContent);
            var table = doc.ToModel();

            var bumpversionConfiguration = table["bumpversion"] as TomlTable;
            globalConfiguration = this.GetGlobalConfiguration(bumpversionConfiguration);

            if (bumpversionConfiguration.TryGetToml("file", out var tomlObject))
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

            Regex expresion = new Regex(globalConfiguration.Parse, RegexOptions.Compiled);

            var match = expresion.Match(globalConfiguration.CurrentVersion);

            int major, minor, patch;
            if (match.Success)
            {
                major = int.Parse(match.Groups["major"].Value);
                minor = int.Parse(match.Groups["minor"].Value);
                patch = int.Parse(match.Groups["patch"].Value);

                var currentVersion = this.FormatVersion(globalConfiguration.Serialize, major, minor, patch);

                switch (this.Part)
                {
                    case "patch":
                        patch += 1;
                        break;
                    case "minor":
                        patch = 0;
                        minor += 1;
                        break;
                    case "major":
                        patch = 0;
                        minor = 0;
                        major += 1;
                        break;
                    default:
                        throw new NotImplementedException($"Bump not implemented for part \"{this.Part}\"");
                }

                var newVersion = this.FormatVersion(globalConfiguration.Serialize, major, minor, patch);

                var message = globalConfiguration.Message.Replace("{current_version}", currentVersion).Replace("{new_version}", newVersion);

                var tagName = globalConfiguration.TagName.Replace("{new_version}", newVersion);

                foreach (var fileConfiguration in fileConfigurations)
                {
                    var file = File.ReadAllText(fileConfiguration.File);
                    var search = fileConfiguration.Search.Replace("{current_version}", currentVersion);
                    var replace = fileConfiguration.Replace.Replace("{new_version}", newVersion);

                    file = file.Replace(search, replace);

                    File.WriteAllText(fileConfiguration.File, file);
                }

                bumpVersionConfigFileContent = bumpVersionConfigFileContent.Replace($"\"{currentVersion}\"", $"\"{newVersion}\"");

                File.WriteAllText(ConfigurationFileName, bumpVersionConfigFileContent);

                var filesToCommit = fileConfigurations.Select(config => config.File).Append(ConfigurationFileName);

                if (globalConfiguration.Commit)
                {
                    this.repo.Commit(message, filesToCommit.ToArray());
                    if (globalConfiguration.Tag)
                    {
                        this.repo.Tag(tagName, message);
                    }
                }

                Console.WriteLine(message);
            }

            return 0;
        }

        private string FormatVersion(string input, int major, int minor, int patch)
        {
            Dictionary<string, string> replacements = new Dictionary<string, string>
                {
                    { "{major}", major.ToString() },
                    { "{minor}", minor.ToString() },
                    { "{patch}", patch.ToString() },
                };

            foreach (var (key, value) in replacements)
            {
                input = input.Replace(key, value);
            }

            return input;
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

        private GlobalConfiguration GetGlobalConfiguration(TomlTable bumpversionConfiguration)
        {
            var globalConfiguration = new GlobalConfiguration();

            globalConfiguration.CurrentVersion = bumpversionConfiguration["current_version"] as string;

            bumpversionConfiguration.TryGetValue("parse", out var parseObject);
            globalConfiguration.Parse = parseObject as string ?? GlobalConfiguration.Defaults.Parse;

            bumpversionConfiguration.TryGetValue("serialize", out var serializeObject);
            globalConfiguration.Serialize = serializeObject as string ?? GlobalConfiguration.Defaults.Serialize;

            bumpversionConfiguration.TryGetValue("new_version", out var newVersionObject);
            globalConfiguration.NewVersion = newVersionObject as string;

            if (bumpversionConfiguration.TryGetValue("tag", out var tagObject))
            {
                globalConfiguration.Tag = (bool)tagObject;
            }
            else
            {
                globalConfiguration.Tag = GlobalConfiguration.Defaults.Tag;
            }

            if (bumpversionConfiguration.TryGetValue("sign_tags", out var signTagsObject))
            {
                globalConfiguration.SignTags = (bool)signTagsObject;
            }
            else
            {
                globalConfiguration.SignTags = GlobalConfiguration.Defaults.SignTags;
            }

            bumpversionConfiguration.TryGetValue("tag_name", out var tagNameObj);
            globalConfiguration.TagName = tagNameObj as string ?? GlobalConfiguration.Defaults.TagName;

            if (bumpversionConfiguration.TryGetValue("commit", out var commitObject))
            {
                globalConfiguration.Commit = (bool)commitObject;
            }
            else
            {
                globalConfiguration.Commit = GlobalConfiguration.Defaults.Commit;
            }

            bumpversionConfiguration.TryGetValue("message", out var messageObj);
            globalConfiguration.Message = messageObj as string ?? GlobalConfiguration.Defaults.Message;

            bumpversionConfiguration.TryGetValue("commit_args", out var commitArgsObj);
            globalConfiguration.CommitArgs = commitArgsObj as string;

            return globalConfiguration;
        }
    }
}
