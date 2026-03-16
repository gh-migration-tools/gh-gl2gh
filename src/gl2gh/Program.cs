using GitHub.Migration.Cli;
using GitHub.Migration.Services;
using GitLabToGitHub.Cli;
using GitLabToGitHub.Services;
using Microsoft.Extensions.DependencyInjection;
using OctoshiftCLI;
using OctoshiftCLI.Contracts;
using OctoshiftCLI.Extensions;
using OctoshiftCLI.Factories;
using OctoshiftCLI.Services;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using GitHubApiFactory = GitLabToGitHub.Factories.GitHubApiFactory;
using VersionChecker = GitLabToGitHub.Services.VersionChecker;

namespace GitLabToGitHub;

[ExcludeFromCodeCoverage]
public class Program
{
    private static readonly OctoLogger Logger = new();

    private static async Task Main(string[] args)
    {
        Logger.LogDebug("Execution Started");

        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddSingleton(Logger)
            .AddSingleton<EnvironmentVariableProvider>(sp => sp.GetRequiredService<EnvironmentVariableProviderExtended>())
            .AddSingleton<EnvironmentVariableProviderExtended>()
            .AddSingleton<GitHubApiFactory>()
            .AddSingleton<RetryPolicy>()
            .AddSingleton<BasicHttpClient>()
            .AddSingleton<GithubStatusApi>()
            .AddSingleton<VersionChecker>()
            .AddSingleton<HttpDownloadServiceFactory>()
            .AddSingleton<DateTimeProvider>()
            .AddSingleton<WarningsCountLogger>()
            .AddSingleton<FileSystemProvider>()
            .AddSingleton<ConfirmationService>()
            .AddSingleton<IVersionProvider, VersionChecker>(sp => sp.GetRequiredService<VersionChecker>())
            .AddSingleton<ICliClient, CliClient>()
            .AddSingleton<ICliLogger, CliLogger>()
            .AddSingleton<IProcessRunner, ProcessRunner>()
            .AddHttpClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var parser = BuildParser(serviceProvider);

        SetContext(parser.Parse(args));

        WarnIfNotUsingExtension();

        try
        {
            await GitHubStatusCheck(serviceProvider).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Could not check GitHub availability from githubstatus.com. See https://www.githubstatus.com for details.");
            Logger.LogVerbose(ex.ToString());
        }

        try
        {
            await LatestVersionCheck(serviceProvider).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Could not retrieve latest gl2gh CLI version from github.com, please ensure you are using the latest version by running: gh extension upgrade gl2gh");
            Logger.LogVerbose(ex.ToString());
        }

        await parser.InvokeAsync(args).ConfigureAwait(false);
    }

    private static void WarnIfNotUsingExtension()
    {
        if (!Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory).EndsWith(Path.Join("extensions", "gh-gl2gh"), StringComparison.Ordinal))
        {
            Logger.LogWarning("You are not running the gl2gh CLI as a gh extension. This is not recommended, please run: gh extension install github/gh-gl2gh");
        }
    }

    private static void SetContext(ParseResult parseResult)
    {
        CliContext.RootCommand = "gl2gh";
        CliContext.ExecutingCommand = parseResult.CommandResult.Command.Name;
    }

    private static async Task GitHubStatusCheck(ServiceProvider sp)
    {
        var envProvider = sp.GetRequiredService<EnvironmentVariableProvider>();

        if (envProvider.SkipStatusCheck()?.ToUpperInvariant() is "TRUE" or "1")
        {
            Logger.LogInformation("Skipped GitHub status check due to GEI_SKIP_STATUS_CHECK environment variable");
            return;
        }

        var githubStatusApi = sp.GetRequiredService<GithubStatusApi>();

        if (await githubStatusApi.GetUnresolvedIncidentsCount().ConfigureAwait(false) > 0)
        {
            Logger.LogWarning("GitHub is currently experiencing availability issues.  See https://www.githubstatus.com for details.");
        }
    }

    private static async Task LatestVersionCheck(ServiceProvider sp)
    {
        var envProvider = sp.GetRequiredService<EnvironmentVariableProvider>();

        if (envProvider.SkipVersionCheck()?.ToUpperInvariant() is "TRUE" or "1")
        {
            Logger.LogInformation("Skipped latest version check due to GEI_SKIP_VERSION_CHECK environment variable");
            return;
        }

        var versionChecker = sp.GetRequiredService<VersionChecker>();

        if (await versionChecker.IsLatest().ConfigureAwait(false))
        {
            Logger.LogInformation($"You are running an up-to-date version of the gl2gh CLI [v{versionChecker.GetCurrentVersion()}]");
        }
        else
        {
            Logger.LogWarning($"You are running an old version of the gl2gh CLI [v{versionChecker.GetCurrentVersion()}]. The latest version is v{await versionChecker.GetLatestVersion().ConfigureAwait(false)}.");
            Logger.LogWarning("Please update by running: gh extension upgrade gl2gh");
        }
    }

    private static Parser BuildParser(ServiceProvider serviceProvider)
    {
        var root = new RootCommand("Automate end-to-end GitLab Repos to GitHub migrations.")
            .AddCommands(serviceProvider);
        var commandLineBuilder = new CommandLineBuilder(root);

        return commandLineBuilder
            .UseDefaults()
            .UseExceptionHandler((ex, _) =>
            {
                Logger.LogError(ex);
                Environment.ExitCode = 1;
            }, 1)
            .Build();
    }
}
