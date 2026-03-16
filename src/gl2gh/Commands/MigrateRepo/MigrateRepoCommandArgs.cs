using OctoshiftCLI.Commands;
using OctoshiftCLI.Commands.MigrateRepo;
using OctoshiftCLI.Extensions;

namespace GitLabToGitHub.Commands.MigrateRepo;

public sealed class MigrateRepoCommandArgs : MigrateRepoCommandArgsBase
{
    public string? DockerImage { get; set; }
    public string GitLabServerUrl { get; set; } = null!;
    public string GitLabApiUrl { get; set; } = null!;
    [Secret]
    public string GitLabPat { get; set; } = null!;
    public string? GitLabUsername { get; set; }
    public string GitLabNamespace { get; set; } = null!;
    public string GitLabProject { get; set; } = null!;
    public string? GitLabOnly { get; set; }
    public string? GitLabExcept { get; set; }
    public string? GitLabLockProjects { get; set; }
    public string? GitLabWithoutRenumbering { get; set; }
    public string? GitLabManifest { get; set; }
    public bool GitLabSslNoVerify { get; set; }
    public bool GitLabDebug { get; set; }

    public override bool ShouldGenerateArchive() =>
        GitLabApiUrl.HasValue() && !ArchiveUrl.HasValue();
}
