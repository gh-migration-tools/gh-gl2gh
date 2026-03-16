using GitHub.Migration.Cli;

namespace GitLabToGitHub.Cli.Commands;

public sealed class DockerImagesCommand : ICliCommand
{
    public string CliCommand => "docker images";

    [CliCommandOption("--filter", isRequired: true)]
    public required string Filter { get; init; } = null!;
}
