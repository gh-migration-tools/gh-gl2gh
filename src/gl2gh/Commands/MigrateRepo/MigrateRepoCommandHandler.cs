using GitHub.Migration.Cli;
using GitLabToGitHub.Cli.Commands;
using GitLabToGitHub.Services;
using OctoshiftCLI;
using OctoshiftCLI.Commands.MigrateRepo;
using OctoshiftCLI.Extensions;
using OctoshiftCLI.Services;
using System.Globalization;

namespace GitLabToGitHub.Commands.MigrateRepo;

public sealed class MigrateRepoCommandHandler : MigrateRepoCommandHandlerBase<MigrateRepoCommandArgs>
{
    private readonly OctoLogger _logger;
    private readonly GithubApiExtended _githubApi;
    private readonly EnvironmentVariableProviderExtended _environmentVariableProvider;
    private readonly ICliClient _cliClient;

    private ExportMethod _exportMethod = ExportMethod.None;

    public MigrateRepoCommandHandler(
        OctoLogger logger,
        GithubApiExtended githubApi,
        EnvironmentVariableProviderExtended environmentVariableProvider,
        FileSystemProvider fileSystemProvider,
        WarningsCountLogger warningsCountLogger,
        ICliClient cliClient)
        : base(logger, githubApi, environmentVariableProvider, fileSystemProvider, warningsCountLogger)
    {
        ArgumentNullException.ThrowIfNull(cliClient);

        _logger = logger;
        _githubApi = githubApi;
        _environmentVariableProvider = environmentVariableProvider;
        _cliClient = cliClient;
    }

    protected override async Task ValidateOptionsAsync(MigrateRepoCommandArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // Validate args
        if (args.ShouldGenerateArchive())
        {
            if (HasGitLabUsername(args))
            {
                throw new OctoshiftCliException("GitLab username must be either set as GL_USERNAME environment variable or passed as --gl-username.");
            }

            if (HasGitLabPat(args))
            {
                throw new OctoshiftCliException("GitLab PAT must be either set as GL_PAT environment variable or passed as --gl-pat.");
            }

            if (string.IsNullOrWhiteSpace(args.ArchivePath))
            {
                args.ArchivePath = ".";
            }

            Directory.CreateDirectory(args.ArchivePath);
        }

        // Validate dependencies
        // If the Docker image is not provided, we will assume that the gl_exporter is installed
        if (string.IsNullOrWhiteSpace(args.DockerImage))
        {
            await ValidateGlExporterAsync().ConfigureAwait(false);
            _exportMethod = ExportMethod.GlExporter;
        }
        else
        {
            await ValidateDockerAsync().ConfigureAwait(false);
            await ValidateDockerImageAsync(args.DockerImage).ConfigureAwait(false);
            _exportMethod = ExportMethod.Docker;
        }
    }

    protected override async Task<string> GenerateArchiveAsync(MigrateRepoCommandArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var exportId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var archiveFileName = $"gitlab-export-{exportId}.tar.gz";

        ICliCommand command = null!;
        List<KeyValuePair<string, string>>? environmentVariables = null;

        var glExporterCommand = GetGlExporterCommand(args, archiveFileName);
        var glExporterEnvironmentVariables = new List<KeyValuePair<string, string>>
        {
            new("GITLAB_API_ENDPOINT", args.GitLabApiUrl),
            new("GITLAB_USERNAME", args.GitLabUsername!),
            new("GITLAB_API_PRIVATE_TOKEN", args.GitLabPat)
        };

        if (_exportMethod == ExportMethod.GlExporter)
        {
            command = glExporterCommand;
            environmentVariables = glExporterEnvironmentVariables;
        }
        else if (_exportMethod == ExportMethod.Docker)
        {
            command = new DockerRunCommand
            {
                Remove = true,
                Interactive = true,
                EnvironmentVariables = glExporterEnvironmentVariables.ToKeyValueList(),
                Volumes = ["${PWD}:/workspace"],
                Image = args.DockerImage!,
                Command = glExporterCommand.ToCommandString()
            };
        }

        _logger.LogInformation($"Export started. Export ID: {exportId}");

        var exitCode = await _cliClient
            .RunCommandAsync(
                command,
                args.ArchivePath,
                environmentVariables)
            .ConfigureAwait(false);

        if (exitCode != 0)
        {
            throw new OctoshiftCliException("GitLab export failed");
        }

        var archiveFilePath = Path.GetFullPath(Path.Combine(args.ArchivePath!, archiveFileName));
        _logger.LogInformation($"Export completed. Your migration archive should be ready at {archiveFilePath}");

        return archiveFilePath;
    }

    protected override string GetRepoUrl(MigrateRepoCommandArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.GitLabServerUrl.HasValue() && args.GitLabNamespace.HasValue() && args.GitLabProject.HasValue()
            ? $"{args.GitLabServerUrl.TrimEnd('/')}/{args.GitLabNamespace.EscapeDataString()}/{args.GitLabProject.EscapeDataString()}"
            : "https://not-used";
    }

    protected override Task<string> CreateMigrationSource(string orgId) =>
        _githubApi.CreateGitLabMigrationSource(orgId);

    protected override Task<string> StartMigration(
        string migrationSourceId,
        string repoUrl,
        string orgId,
        string repo,
        string token,
        string? archiveUrl,
        string? targetRepoVisibility = null) =>
        _githubApi.StartGitLabMigration(
            migrationSourceId,
            repoUrl,
            orgId,
            repo,
            token,
            archiveUrl,
            targetRepoVisibility);

    protected override Task<(string State, string RepositoryName, int WarningsCount, string FailureReason, string MigrationLogUrl)> GetMigration(string migrationId) =>
        _githubApi.GetGitLabMigration(migrationId);

    private bool HasGitLabUsername(MigrateRepoCommandArgs args)
    {
        var value = args.GitLabUsername.HasValue()
            ? args.GitLabUsername!
            : _environmentVariableProvider.GitLabUsername(false);

        return string.IsNullOrWhiteSpace(value);
    }

    private bool HasGitLabPat(MigrateRepoCommandArgs args)
    {
        var value = args.GitLabPat.HasValue()
            ? args.GitLabPat
            : _environmentVariableProvider.GitLabPat(false);

        return string.IsNullOrWhiteSpace(value);
    }

    private async Task ValidateGlExporterAsync()
    {
        var command = new GlExporterVersionCommand();
        var outputData = new List<string>();
        var exitCode = await _cliClient
            .RunCommandAsync(
                command,
                outputDataReceived: outputData.Add)
            .ConfigureAwait(false);

        var isGlExporterInstalled = exitCode == 0 &&
                                     outputData.Any(str => str.Contains("GitLab Exporter", StringComparison.OrdinalIgnoreCase));

        if (!isGlExporterInstalled)
        {
            throw new OctoshiftCliException("gl_exporter is not installed.");
        }
    }

    private async Task ValidateDockerAsync()
    {
        var command = new DockerVersionCommand();
        var outputData = new List<string>();
        var exitCode = await _cliClient
            .RunCommandAsync(
                command,
                outputDataReceived: outputData.Add)
            .ConfigureAwait(false);

        var isDockerInstalled = exitCode == 0 &&
                                outputData.Any(str => str.Contains("Docker version", StringComparison.OrdinalIgnoreCase));

        if (!isDockerInstalled)
        {
            throw new OctoshiftCliException("Docker is not installed.");
        }
    }

    private async Task ValidateDockerImageAsync(string dockerImage)
    {
        var command = new DockerImagesCommand
        {
            Filter = $"reference=\"{dockerImage}\""
        };
        var outputData = new List<string>();
        var exitCode = await _cliClient
            .RunCommandAsync(
                command,
                outputDataReceived: outputData.Add)
            .ConfigureAwait(false);

        var isDockerInstalled = exitCode == 0 &&
                                outputData.Any(str => str.Contains(dockerImage, StringComparison.OrdinalIgnoreCase));

        if (!isDockerInstalled)
        {
            throw new OctoshiftCliException($"The Docker image {dockerImage} is not installed.");
        }
    }

    private static GlExporterCommand GetGlExporterCommand(MigrateRepoCommandArgs args, string outFile) =>
        new()
        {
            OutFile = outFile,
            Namespace = args.GitLabNamespace,
            Project = args.GitLabProject,
            Only = args.GitLabOnly,
            Except = args.GitLabExcept,
            LockProjects = args.GitLabLockProjects,
            WithoutRenumbering = args.GitLabWithoutRenumbering,
            Manifest = args.GitLabManifest,
            SslNoVerify = args.GitLabSslNoVerify,
            Debug = args.GitLabDebug
        };

    private enum ExportMethod
    {
        None,
        GlExporter,
        Docker
    }
}
