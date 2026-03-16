using GitHub.Migration.Cli;

namespace GitLabToGitHub.Cli.Commands;

public sealed class DockerRunCommand : ICliCommand
{
    public string CliCommand => "docker run";

    [CliCommandOption("--rm")]
    public bool Remove { get; set; }

    [CliCommandOption("--interactive")]
    public bool Interactive { get; set; }

    [CliCommandOption("--env")]
    public IReadOnlyList<string>? EnvironmentVariables { get; set; }

    [CliCommandOption("--volume")]
    public IReadOnlyList<string>? Volumes { get; set; }

    [CliCommandArgument]
    public required string Image { get; init; }

    [CliCommandArgument]
    public required string Command { get; init; }
}
