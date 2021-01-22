namespace BumpVersion
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using global::BumpVersion.Configuration;
    using global::BumpVersion.Utils;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class BumpVersion
    {
        private const string ConfigurationFileName = ".bumpversion.cfg";

        private readonly ILogger logger;
        private readonly IRepo repo;
        private readonly IBumpConfigurationManager bumpConfigurationManager;

        public BumpVersion(ILogger<BumpVersion> logger, IRepo repo, IBumpConfigurationManager bumpConfigurationManager)
        {
            this.logger = logger;
            this.repo = repo;
            this.bumpConfigurationManager = bumpConfigurationManager;
        }

        [Argument(0)]
        [Required]
        public string Part { get; }

        public static string GetVersion()
            => typeof(BumpVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
              .AddSingleton<IConsole>(PhysicalConsole.Singleton)
              .AddScoped<IRepo, Repo>()
              .AddScoped<IBumpConfigurationManager, BumpConfigurationManager>()
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

            this.bumpConfigurationManager.ReadConfig(ConfigurationFileName);
            var globalConfiguration = this.bumpConfigurationManager.GetGlobalConfiguration();
            var fileConfigurations = this.bumpConfigurationManager.GetFileConfigurations();

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

                var filesToCommit = fileConfigurations.Select(config => config.File).Append(ConfigurationFileName);

                this.bumpConfigurationManager.UpdateConfigurationVersion(currentVersion, newVersion);

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
    }
}
