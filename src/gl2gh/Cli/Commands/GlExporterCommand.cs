using GitHub.Migration.Cli;

namespace GitLabToGitHub.Cli.Commands;

public sealed class GlExporterCommand : ICliCommand
{
    public string CliCommand => "gl_exporter";

    [CliCommandOption("--out-file", isRequired: true)]
    public required string OutFile { get; init; }

    [CliCommandOption("--namespace", isRequired: true)]
    public required string Namespace { get; init; }

    [CliCommandOption("--project", isRequired: true)]
    public required string Project { get; init; }

    [CliCommandOption("--only")]
    public string? Only { get; set; }

    [CliCommandOption("--except")]
    public string? Except { get; set; }

    [CliCommandOption("--lock-projects")]
    public string? LockProjects { get; set; }

    [CliCommandOption("--without-renumbering")]
    public string? WithoutRenumbering { get; set; }

    [CliCommandOption("--manifest")]
    public string? Manifest { get; set; }

    [CliCommandOption("--ssl-no-verify")]
    public bool SslNoVerify { get; set; }

    [CliCommandOption("--debug")]
    public bool Debug { get; set; }
}
