using GitHub.Migration.Cli;

namespace GitLabToGitHub.Cli.Commands;

public sealed class DockerVersionCommand : ICliCommand
{
    public string CliCommand => "docker --version";
}
